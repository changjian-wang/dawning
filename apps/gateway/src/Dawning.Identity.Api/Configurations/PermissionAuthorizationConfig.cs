using Dawning.Identity.Api.Security;
using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// Permission authorization configuration
    /// </summary>
    public static class PermissionAuthorizationConfig
    {
        /// <summary>
        /// Add permission authorization configuration
        /// </summary>
        public static IServiceCollection AddPermissionAuthorization(
            this IServiceCollection services
        )
        {
            // Register custom policy provider
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // Register permission validation handler
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }
    }
}
