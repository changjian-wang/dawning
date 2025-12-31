using System.Security.Claims;
using Dawning.Identity.Constants;

namespace Dawning.Identity.Extensions;

/// <summary>
/// ClaimsPrincipal extension methods
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.UserId);
    }

    /// <summary>
    /// Gets the user ID (Guid)
    /// </summary>
    public static Guid? GetUserIdAsGuid(this ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();
        return Guid.TryParse(userId, out var guid) ? guid : null;
    }

    /// <summary>
    /// Gets the username
    /// </summary>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.UserName)
            ?? principal.FindFirstValue(ClaimTypes.Name);
    }

    /// <summary>
    /// Gets the email
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Gets all roles
    /// </summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal
            .FindAll(DawningClaimTypes.Role)
            .Concat(principal.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Distinct();
    }

    /// <summary>
    /// Checks if the user has the specified role
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.GetRoles().Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has any of the specified roles
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        var userRoles = principal.GetRoles().ToHashSet(StringComparer.OrdinalIgnoreCase);
        return roles.Any(r => userRoles.Contains(r));
    }

    /// <summary>
    /// Gets all permissions
    /// </summary>
    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
    {
        return principal.FindAll(DawningClaimTypes.Permission).Select(c => c.Value).Distinct();
    }

    /// <summary>
    /// Checks if the user has the specified permission
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        return principal.GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has any of the specified permissions
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
    {
        var userPermissions = principal
            .GetPermissions()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return permissions.Any(p => userPermissions.Contains(p));
    }

    /// <summary>
    /// Gets the tenant ID
    /// </summary>
    public static string? GetTenantId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.TenantId);
    }

    /// <summary>
    /// Gets the tenant ID (Guid)
    /// </summary>
    public static Guid? GetTenantIdAsGuid(this ClaimsPrincipal principal)
    {
        var tenantId = principal.GetTenantId();
        return Guid.TryParse(tenantId, out var guid) ? guid : null;
    }

    /// <summary>
    /// Gets the tenant code
    /// </summary>
    public static string? GetTenantCode(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.TenantCode);
    }

    /// <summary>
    /// Checks if the user is a host administrator (can access all tenants)
    /// </summary>
    public static bool IsHost(this ClaimsPrincipal principal)
    {
        var isHost = principal.FindFirstValue(DawningClaimTypes.IsHost);
        return bool.TryParse(isHost, out var result) && result;
    }

    /// <summary>
    /// Checks if the user is a super administrator
    /// </summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal principal)
    {
        return principal.HasRole(DawningRoles.SuperAdmin);
    }

    /// <summary>
    /// Checks if the user is an administrator (including super administrator)
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.HasAnyRole(DawningRoles.Admin, DawningRoles.SuperAdmin);
    }

    /// <summary>
    /// Gets the client ID
    /// </summary>
    public static string? GetClientId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(DawningClaimTypes.ClientId);
    }

    /// <summary>
    /// Gets all scopes
    /// </summary>
    public static IEnumerable<string> GetScopes(this ClaimsPrincipal principal)
    {
        return principal.FindAll(DawningClaimTypes.Scope).Select(c => c.Value).Distinct();
    }
}
