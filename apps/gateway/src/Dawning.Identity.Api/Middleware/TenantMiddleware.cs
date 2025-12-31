using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// Tenant resolution middleware
    /// Identifies tenant from request and sets tenant context
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        /// <summary>
        /// Request header name for tenant ID
        /// </summary>
        public const string TenantHeader = "X-Tenant-Id";

        /// <summary>
        /// Request header name for tenant code
        /// </summary>
        public const string TenantCodeHeader = "X-Tenant-Code";

        /// <summary>
        /// Tenant claim type in JWT
        /// </summary>
        public const string TenantClaimType = "tenant_id";

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITenantContext tenantContext,
            ITenantService tenantService
        )
        {
            try
            {
                // Resolve tenant information
                var (tenantId, tenantName, isHost) = await ResolveTenantAsync(
                    context,
                    tenantService
                );

                // Set tenant context
                tenantContext.SetTenant(tenantId, tenantName, isHost);

                if (tenantId.HasValue)
                {
                    _logger.LogDebug("Request tenant: {TenantId} ({TenantName})", tenantId, tenantName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve tenant");
            }

            await _next(context);
        }

        /// <summary>
        /// Resolve tenant information
        /// Priority: JWT claim > Request header > Subdomain
        /// </summary>
        private async Task<(Guid? TenantId, string? TenantName, bool IsHost)> ResolveTenantAsync(
            HttpContext context,
            ITenantService tenantService
        )
        {
            // 1. Get tenant ID from JWT Token
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Check if user is host user (super admin)
                var isHost = context.User.IsInRole("super_admin");

                var tenantClaim = context.User.FindFirst(TenantClaimType);
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var jwtTenantId))
                {
                    var tenant = await tenantService.GetAsync(jwtTenantId);
                    if (tenant != null)
                    {
                        return (jwtTenantId, tenant.Name, isHost);
                    }
                }

                // Super admin can switch tenant via request header
                if (isHost)
                {
                    var switchTenant = await ResolveTenantFromHeaderAsync(context, tenantService);
                    if (switchTenant.TenantId.HasValue)
                    {
                        return (switchTenant.TenantId, switchTenant.TenantName, true);
                    }
                    // When super admin doesn't specify tenant, set to host mode (can access all data)
                    return (null, null, true);
                }
            }

            // 2. Get tenant from request header
            var headerTenant = await ResolveTenantFromHeaderAsync(context, tenantService);
            if (headerTenant.TenantId.HasValue)
            {
                return headerTenant;
            }

            // 3. Get tenant from subdomain
            var host = context.Request.Host.Host;
            var tenant2 = await tenantService.ResolveTenantAsync(null, host);
            if (tenant2 != null)
            {
                return (tenant2.Id, tenant2.Name, false);
            }

            // No tenant resolved
            return (null, null, false);
        }

        private async Task<(
            Guid? TenantId,
            string? TenantName,
            bool IsHost
        )> ResolveTenantFromHeaderAsync(HttpContext context, ITenantService tenantService)
        {
            // Try to get tenant ID from request header
            if (context.Request.Headers.TryGetValue(TenantHeader, out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader.ToString(), out var headerTenantId))
                {
                    var tenant = await tenantService.GetAsync(headerTenantId);
                    if (tenant != null && tenant.IsActive)
                    {
                        return (headerTenantId, tenant.Name, false);
                    }
                }
            }

            // Try to get tenant code from request header
            if (context.Request.Headers.TryGetValue(TenantCodeHeader, out var tenantCodeHeader))
            {
                var tenantCode = tenantCodeHeader.ToString();
                if (!string.IsNullOrWhiteSpace(tenantCode))
                {
                    var tenant = await tenantService.GetByCodeAsync(tenantCode);
                    if (tenant != null && tenant.IsActive)
                    {
                        return (tenant.Id, tenant.Name, false);
                    }
                }
            }

            // Try to get from query parameter
            if (context.Request.Query.TryGetValue("tenant", out var tenantQuery))
            {
                var tenantCode = tenantQuery.ToString();
                if (!string.IsNullOrWhiteSpace(tenantCode))
                {
                    var tenant = await tenantService.GetByCodeAsync(tenantCode);
                    if (tenant != null && tenant.IsActive)
                    {
                        return (tenant.Id, tenant.Name, false);
                    }
                }
            }

            return (null, null, false);
        }
    }

    /// <summary>
    /// Tenant middleware extension methods
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}
