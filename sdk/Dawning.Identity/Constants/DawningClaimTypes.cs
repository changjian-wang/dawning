namespace Dawning.Identity.Constants;

/// <summary>
/// Dawning authentication related constants
/// </summary>
public static class DawningClaimTypes
{
    /// <summary>
    /// User ID
    /// </summary>
    public const string UserId = "sub";

    /// <summary>
    /// Username
    /// </summary>
    public const string UserName = "name";

    /// <summary>
    /// Email
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// Role
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Permission
    /// </summary>
    public const string Permission = "permission";

    /// <summary>
    /// Tenant ID
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// Tenant code
    /// </summary>
    public const string TenantCode = "tenant_code";

    /// <summary>
    /// Whether the user is a host administrator
    /// </summary>
    public const string IsHost = "is_host";

    /// <summary>
    /// Client ID
    /// </summary>
    public const string ClientId = "client_id";

    /// <summary>
    /// Scope
    /// </summary>
    public const string Scope = "scope";
}

/// <summary>
/// Predefined roles
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
/// Predefined permissions
/// </summary>
public static class DawningPermissions
{
    // User management
    public const string UserRead = "user.read";
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";

    // Role management
    public const string RoleRead = "role.read";
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string RoleDelete = "role.delete";

    // Permission management
    public const string PermissionRead = "permission.read";
    public const string PermissionManage = "permission.manage";

    // System configuration
    public const string ConfigRead = "config.read";
    public const string ConfigWrite = "config.write";

    // Audit logs
    public const string AuditLogRead = "audit.read";
    public const string AuditLogExport = "audit.export";
}
