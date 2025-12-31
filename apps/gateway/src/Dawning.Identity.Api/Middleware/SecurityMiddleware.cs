using Microsoft.AspNetCore.Antiforgery;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// Security headers middleware - Adds security-related HTTP response headers
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security response headers
            var headers = context.Response.Headers;

            // X-Content-Type-Options: Prevent MIME type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // X-Frame-Options: Prevent clickjacking
            headers["X-Frame-Options"] = "DENY";

            // X-XSS-Protection: Enable XSS filter (legacy browser support)
            headers["X-XSS-Protection"] = "1; mode=block";

            // Referrer-Policy: Control Referrer information sending
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Permissions-Policy: Limit browser features
            headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            // Content-Security-Policy: Content security policy (only for Swagger UI and similar pages)
            if (
                context.Request.Path.StartsWithSegments("/swagger")
                || context.Request.Path == "/"
                || context.Request.Path.StartsWithSegments("/index.html")
            )
            {
                headers["Content-Security-Policy"] =
                    "default-src 'self'; "
                    + "script-src 'self' 'unsafe-inline' 'unsafe-eval'; "
                    + "style-src 'self' 'unsafe-inline'; "
                    + "img-src 'self' data: https:; "
                    + "font-src 'self' data:; "
                    + "connect-src 'self';";
            }

            await _next(context);
        }
    }

    /// <summary>
    /// CSRF Token endpoint middleware - Provides CSRF Token for scenarios requiring Cookie authentication
    /// </summary>
    public class CsrfTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery? _antiforgery;

        public CsrfTokenMiddleware(RequestDelegate next, IAntiforgery? antiforgery = null)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate CSRF Token for GET requests (for frontend SPA to retrieve)
            if (
                _antiforgery != null
                && context.Request.Method == HttpMethods.Get
                && context.Request.Path == "/api/auth/csrf-token"
            )
            {
                var tokens = _antiforgery.GetAndStoreTokens(context);
                context.Response.Headers["X-CSRF-TOKEN"] = tokens.RequestToken;

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    new { token = tokens.RequestToken, headerName = tokens.HeaderName }
                );
                return;
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Security middleware extension methods
    /// </summary>
    public static class SecurityMiddlewareExtensions
    {
        /// <summary>
        /// Add security headers middleware
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        /// <summary>
        /// Add CSRF Token middleware
        /// </summary>
        public static IApplicationBuilder UseCsrfToken(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CsrfTokenMiddleware>();
        }

        /// <summary>
        /// Configure Antiforgery service
        /// </summary>
        public static IServiceCollection AddCsrfProtection(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            return services;
        }
    }
}
