using System.Security.Claims;
using Dawning.Identity.Constants;

namespace Dawning.Identity.Extensions;

/// <summary>
/// ClaimsPrincipal 扩展方法
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// 获取用户ID
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.UserId);
    }

    /// <summary>
    /// 获取用户ID (Guid)
    /// </summary>
    public static Guid? GetUserIdAsGuid(this ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();
        return Guid.TryParse(userId, out var guid) ? guid : null;
    }

    /// <summary>
    /// 获取用户名
    /// </summary>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.UserName)
            ?? principal.FindFirstValue(ClaimTypes.Name);
    }

    /// <summary>
    /// 获取邮箱
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// 获取所有角色
    /// </summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(DawningClaimTypes.Role)
            .Concat(principal.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Distinct();
    }

    /// <summary>
    /// 检查是否有指定角色
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.GetRoles().Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查是否有任意一个角色
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        var userRoles = principal.GetRoles().ToHashSet(StringComparer.OrdinalIgnoreCase);
        return roles.Any(r => userRoles.Contains(r));
    }

    /// <summary>
    /// 获取所有权限
    /// </summary>
    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
    {
        return principal.FindAll(DawningClaimTypes.Permission)
            .Select(c => c.Value)
            .Distinct();
    }

    /// <summary>
    /// 检查是否有指定权限
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        return principal.GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查是否有任意一个权限
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
    {
        var userPermissions = principal.GetPermissions().ToHashSet(StringComparer.OrdinalIgnoreCase);
        return permissions.Any(p => userPermissions.Contains(p));
    }

    /// <summary>
    /// 获取租户ID
    /// </summary>
    public static string? GetTenantId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.TenantId);
    }

    /// <summary>
    /// 获取租户ID (Guid)
    /// </summary>
    public static Guid? GetTenantIdAsGuid(this ClaimsPrincipal principal)
    {
        var tenantId = principal.GetTenantId();
        return Guid.TryParse(tenantId, out var guid) ? guid : null;
    }

    /// <summary>
    /// 获取租户编码
    /// </summary>
    public static string? GetTenantCode(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.TenantCode);
    }

    /// <summary>
    /// 是否为宿主管理员 (可访问所有租户)
    /// </summary>
    public static bool IsHost(this ClaimsPrincipal principal)
    {
        var isHost = principal.FindFirstValue(DawningClaimTypes.IsHost);
        return bool.TryParse(isHost, out var result) && result;
    }

    /// <summary>
    /// 是否为超级管理员
    /// </summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal principal)
    {
        return principal.HasRole(DawningRoles.SuperAdmin);
    }

    /// <summary>
    /// 是否为管理员 (包括超级管理员)
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.HasAnyRole(DawningRoles.Admin, DawningRoles.SuperAdmin);
    }

    /// <summary>
    /// 获取客户端ID
    /// </summary>
    public static string? GetClientId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.ClientId);
    }

    /// <summary>
    /// 获取所有作用域
    /// </summary>
    public static IEnumerable<string> GetScopes(this ClaimsPrincipal principal)
    {
        return principal.FindAll(DawningClaimTypes.Scope)
            .Select(c => c.Value)
            .Distinct();
    }
}
