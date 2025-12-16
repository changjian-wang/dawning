using Microsoft.AspNetCore.Antiforgery;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// 安全头中间件 - 添加安全相关的 HTTP 响应头
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
            // 添加安全响应头
            var headers = context.Response.Headers;

            // X-Content-Type-Options: 防止 MIME 类型嗅探
            headers["X-Content-Type-Options"] = "nosniff";

            // X-Frame-Options: 防止点击劫持
            headers["X-Frame-Options"] = "DENY";

            // X-XSS-Protection: 启用 XSS 过滤器（旧浏览器支持）
            headers["X-XSS-Protection"] = "1; mode=block";

            // Referrer-Policy: 控制 Referrer 信息发送
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Permissions-Policy: 限制浏览器功能
            headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            // Content-Security-Policy: 内容安全策略（仅用于 Swagger UI 等页面）
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
    /// CSRF Token 端点中间件 - 为需要 Cookie 认证的场景提供 CSRF Token
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
            // 为 GET 请求生成 CSRF Token（用于前端 SPA 获取）
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
    /// 安全中间件扩展方法
    /// </summary>
    public static class SecurityMiddlewareExtensions
    {
        /// <summary>
        /// 添加安全头中间件
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        /// <summary>
        /// 添加 CSRF Token 中间件
        /// </summary>
        public static IApplicationBuilder UseCsrfToken(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CsrfTokenMiddleware>();
        }

        /// <summary>
        /// 配置 Antiforgery 服务
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
