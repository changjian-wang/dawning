using Dawning.Identity.Api.Security;
using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// 权限授权配置
    /// </summary>
    public static class PermissionAuthorizationConfig
    {
        /// <summary>
        /// 添加权限授权配置
        /// </summary>
        public static IServiceCollection AddPermissionAuthorization(
            this IServiceCollection services
        )
        {
            // 注册自定义策略提供器
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // 注册权限验证处理器
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }
    }
}
