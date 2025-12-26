using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Attributes;

/// <summary>
/// 要求指定权限的授权特性
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
    /// 所需权限
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// 创建权限要求特性
    /// </summary>
    /// <param name="permissions">所需的权限 (满足任一即可)</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
        // 设置策略名称 (用于授权处理器识别)
        Policy = $"Permission:{string.Join(",", permissions)}";
    }
}

/// <summary>
/// 要求指定角色的授权特性 (增强版)
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
    /// 所需角色
    /// </summary>
    public string[] RequiredRoles { get; }

    /// <summary>
    /// 创建角色要求特性
    /// </summary>
    /// <param name="roles">所需的角色 (满足任一即可)</param>
    public RequireRoleAttribute(params string[] roles)
    {
        RequiredRoles = roles;
        Roles = string.Join(",", roles);
    }
}

/// <summary>
/// 仅允许超级管理员访问
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
/// 仅允许管理员访问 (包括超级管理员)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Roles = $"{Constants.DawningRoles.Admin},{Constants.DawningRoles.SuperAdmin}";
    }
}
