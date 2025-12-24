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
    /// 租户解析中间件
    /// 从请求中识别租户并设置到租户上下文
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        /// <summary>
        /// 租户标识的请求头名称
        /// </summary>
        public const string TenantHeader = "X-Tenant-Id";

        /// <summary>
        /// 租户代码的请求头名称
        /// </summary>
        public const string TenantCodeHeader = "X-Tenant-Code";

        /// <summary>
        /// JWT 中的租户声明类型
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
                // 解析租户信息
                var (tenantId, tenantName, isHost) = await ResolveTenantAsync(
                    context,
                    tenantService
                );

                // 设置租户上下文
                tenantContext.SetTenant(tenantId, tenantName, isHost);

                if (tenantId.HasValue)
                {
                    _logger.LogDebug("请求租户: {TenantId} ({TenantName})", tenantId, tenantName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析租户失败");
            }

            await _next(context);
        }

        /// <summary>
        /// 解析租户信息
        /// 优先级: JWT声明 > 请求头 > 子域名
        /// </summary>
        private async Task<(Guid? TenantId, string? TenantName, bool IsHost)> ResolveTenantAsync(
            HttpContext context,
            ITenantService tenantService
        )
        {
            // 1. 从 JWT Token 中获取租户ID
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // 检查是否是主机用户（超级管理员）
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

                // 超级管理员可以通过请求头切换租户
                if (isHost)
                {
                    var switchTenant = await ResolveTenantFromHeaderAsync(context, tenantService);
                    if (switchTenant.TenantId.HasValue)
                    {
                        return (switchTenant.TenantId, switchTenant.TenantName, true);
                    }
                    // 超级管理员没有指定租户时，设置为主机模式（可以访问所有数据）
                    return (null, null, true);
                }
            }

            // 2. 从请求头获取租户
            var headerTenant = await ResolveTenantFromHeaderAsync(context, tenantService);
            if (headerTenant.TenantId.HasValue)
            {
                return headerTenant;
            }

            // 3. 从子域名获取租户
            var host = context.Request.Host.Host;
            var tenant2 = await tenantService.ResolveTenantAsync(null, host);
            if (tenant2 != null)
            {
                return (tenant2.Id, tenant2.Name, false);
            }

            // 没有解析到租户
            return (null, null, false);
        }

        private async Task<(
            Guid? TenantId,
            string? TenantName,
            bool IsHost
        )> ResolveTenantFromHeaderAsync(HttpContext context, ITenantService tenantService)
        {
            // 尝试从请求头获取租户ID
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

            // 尝试从请求头获取租户代码
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

            // 尝试从查询参数获取
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
    /// 租户中间件扩展方法
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}
