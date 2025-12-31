using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Attributes;

/// <summary>
/// Authorization attribute that requires specified permissions
/// </summary>
/// <example>
/// <code>
/// [RequirePermission("user.create")]
/// public async Task&lt;IActionResult&gt; CreateUser() { }
///
/// [RequirePermission("user.read", "user.list")]
/// public async Task&lt;IActionResult&gt; GetUsers() { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Required permissions
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// Creates a permission requirement attribute
    /// </summary>
    /// <param name="permissions">Required permissions (any one is sufficient)</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
        // Set policy name (for authorization handler identification)
        Policy = $"Permission:{string.Join(",", permissions)}";
    }
}

/// <summary>
/// Authorization attribute that requires specified roles (enhanced version)
/// </summary>
/// <example>
/// <code>
/// [RequireRole("admin", "super_admin")]
/// public async Task&lt;IActionResult&gt; AdminOnly() { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Required roles
    /// </summary>
    public string[] RequiredRoles { get; }

    /// <summary>
    /// Creates a role requirement attribute
    /// </summary>
    /// <param name="roles">Required roles (any one is sufficient)</param>
    public RequireRoleAttribute(params string[] roles)
    {
        RequiredRoles = roles;
        Roles = string.Join(",", roles);
    }
}

/// <summary>
/// Allows only super administrators to access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SuperAdminOnlyAttribute : AuthorizeAttribute
{
    public SuperAdminOnlyAttribute()
    {
        Roles = Constants.DawningRoles.SuperAdmin;
    }
}

/// <summary>
/// Allows only administrators to access (including super administrators)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Roles = $"{Constants.DawningRoles.Admin},{Constants.DawningRoles.SuperAdmin}";
    }
}
