namespace Dawning.Shared.Authentication.Constants;

/// <summary>
/// Dawning 认证相关常量
/// </summary>
public static class DawningClaimTypes
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public const string UserId = "sub";

    /// <summary>
    /// 用户名
    /// </summary>
    public const string UserName = "name";

    /// <summary>
    /// 邮箱
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// 角色
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// 权限
    /// </summary>
    public const string Permission = "permission";

    /// <summary>
    /// 租户ID
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// 租户编码
    /// </summary>
    public const string TenantCode = "tenant_code";

    /// <summary>
    /// 是否为宿主管理员
    /// </summary>
    public const string IsHost = "is_host";

    /// <summary>
    /// 客户端ID
    /// </summary>
    public const string ClientId = "client_id";

    /// <summary>
    /// 作用域
    /// </summary>
    public const string Scope = "scope";
}

/// <summary>
/// 预定义的角色
/// </summary>
public static class DawningRoles
{
    public const string SuperAdmin = "super_admin";
    public const string Admin = "admin";
    public const string User = "user";
    public const string Auditor = "auditor";
    public const string Guest = "guest";
}

/// <summary>
/// 预定义的权限
/// </summary>
public static class DawningPermissions
{
    // 用户管理
    public const string UserRead = "user.read";
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";

    // 角色管理
    public const string RoleRead = "role.read";
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string RoleDelete = "role.delete";

    // 权限管理
    public const string PermissionRead = "permission.read";
    public const string PermissionManage = "permission.manage";

    // 系统配置
    public const string ConfigRead = "config.read";
    public const string ConfigWrite = "config.write";

    // 审计日志
    public const string AuditLogRead = "audit.read";
    public const string AuditLogExport = "audit.export";
}
