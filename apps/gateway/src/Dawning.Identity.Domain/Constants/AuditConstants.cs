namespace Dawning.Identity.Domain.Constants;

/// <summary>
/// Audit log action types
/// </summary>
public static class AuditAction
{
    // CRUD Operations
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Read = "Read";

    // Authentication
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string LoginFailed = "LoginFailed";
    public const string PasswordChange = "PasswordChange";
    public const string PasswordReset = "PasswordReset";

    // Authorization
    public const string AssignPermissions = "AssignPermissions";
    public const string RemovePermissions = "RemovePermissions";
    public const string AssignRoles = "AssignRoles";
    public const string RemoveRoles = "RemoveRoles";

    // System
    public const string Enable = "Enable";
    public const string Disable = "Disable";
    public const string Lock = "Lock";
    public const string Unlock = "Unlock";
    public const string Export = "Export";
    public const string Import = "Import";
}

/// <summary>
/// Audit log entity types
/// </summary>
public static class AuditEntityType
{
    // Administration
    public const string User = "User";
    public const string Role = "Role";
    public const string Permission = "Permission";
    public const string RolePermission = "RolePermission";
    public const string UserRole = "UserRole";

    // Authentication
    public const string Token = "Token";
    public const string Session = "Session";
    public const string Application = "Application";

    // Gateway
    public const string Route = "Route";
    public const string RateLimitRule = "RateLimitRule";
    public const string IpWhitelist = "IpWhitelist";
    public const string IpBlacklist = "IpBlacklist";

    // System
    public const string SystemConfig = "SystemConfig";
    public const string AuditLog = "AuditLog";
    public const string AlertRule = "AlertRule";
}
