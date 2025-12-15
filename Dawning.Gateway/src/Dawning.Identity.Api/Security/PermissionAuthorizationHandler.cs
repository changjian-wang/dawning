using Dawning.Identity.Domain.Interfaces.Administration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Dawning.Identity.Api.Security
{
    /// <summary>
    /// 权限验证处理器，负责验证用户是否拥有指定权限
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
            // 获取用户ID
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirst("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Permission check failed: User ID not found in claims");
                return;
            }

            // 检查是否是超级管理员（超级管理员拥有所有权限）
            if (context.User.IsInRole("super_admin"))
            {
                _logger.LogDebug(
                    "Permission {Permission} granted to super_admin user {UserId}",
                    requirement.Permission,
                    userId
                );
                context.Succeed(requirement);
                return;
            }

            // 使用 scope 获取仓储服务
            using var scope = _serviceProvider.CreateScope();
            var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
            var rolePermissionRepository = scope.ServiceProvider.GetRequiredService<IRolePermissionRepository>();

            try
            {
                // Step 1: 获取用户的所有角色
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

                // Step 2: 检查用户的任意角色是否拥有该权限
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
