using System.Security.Claims;
using Dawning.Identity.Domain.Interfaces.Administration;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;

namespace Dawning.Identity.Api.Security
{
    /// <summary>
    /// Permission authorization handler, responsible for verifying if user has specified permissions
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IServiceProvider serviceProvider,
            ILogger<PermissionAuthorizationHandler> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement
        )
        {
            // Get user ID
            var userIdClaim =
                context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Permission check failed: User ID not found in claims");
                return;
            }

            // Check if super admin (super admin has all permissions)
            // Note: OpenIddict uses "role" claim type, need to check claims directly
            var roles = context
                .User.Claims.Where(c =>
                    c.Type == OpenIddictConstants.Claims.Role
                    || c.Type == ClaimTypes.Role
                    || c.Type == "role"
                )
                .Select(c => c.Value)
                .ToList();

            if (roles.Contains("super_admin"))
            {
                _logger.LogDebug(
                    "Permission {Permission} granted to super_admin user {UserId}",
                    requirement.Permission,
                    userId
                );
                context.Succeed(requirement);
                return;
            }

            _logger.LogDebug(
                "User {UserId} roles from token: [{Roles}]",
                userId,
                string.Join(", ", roles)
            );

            // Use scope to get repository services
            using var scope = _serviceProvider.CreateScope();
            var userRoleRepository =
                scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
            var rolePermissionRepository =
                scope.ServiceProvider.GetRequiredService<IRolePermissionRepository>();

            try
            {
                // Step 1: Get all roles of the user
                var userRoles = await userRoleRepository.GetUserRolesAsync(userId);
                if (!userRoles.Any())
                {
                    _logger.LogWarning(
                        "Permission {Permission} denied for user {UserId}: no roles assigned",
                        requirement.Permission,
                        userId
                    );
                    return;
                }

                // Step 2: Check if any of the user's roles have this permission
                foreach (var role in userRoles)
                {
                    var hasPermission = await rolePermissionRepository.HasPermissionAsync(
                        role.Id,
                        requirement.Permission
                    );

                    if (hasPermission)
                    {
                        _logger.LogDebug(
                            "Permission {Permission} granted to user {UserId} via role {RoleName}",
                            requirement.Permission,
                            userId,
                            role.Name
                        );
                        context.Succeed(requirement);
                        return;
                    }
                }

                _logger.LogWarning(
                    "Permission {Permission} denied for user {UserId}",
                    requirement.Permission,
                    userId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking permission {Permission} for user {UserId}",
                    requirement.Permission,
                    userId
                );
            }
        }
    }
}
