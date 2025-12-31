using System.Security.Claims;
using Dawning.Identity.Extensions;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity;

/// <summary>
/// Current user interface - convenient for getting current logged-in user information in the service layer
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// User ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// User ID (Guid)
    /// </summary>
    Guid? UserIdAsGuid { get; }

    /// <summary>
    /// Username
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Email
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// All roles
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// All permissions
    /// </summary>
    IEnumerable<string> Permissions { get; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Tenant ID (Guid)
    /// </summary>
    Guid? TenantIdAsGuid { get; }

    /// <summary>
    /// Tenant code
    /// </summary>
    string? TenantCode { get; }

    /// <summary>
    /// Whether the user is a host administrator
    /// </summary>
    bool IsHost { get; }

    /// <summary>
    /// Whether the user is a super administrator
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Whether the user is an administrator
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Checks if the user has the specified role
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Checks if the user has the specified permission
    /// </summary>
    bool HasPermission(string permission);

    /// <summary>
    /// Gets the original ClaimsPrincipal
    /// </summary>
    ClaimsPrincipal? Principal { get; }
}

/// <summary>
/// Current user implementation
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? UserId => User?.GetUserId();

    public Guid? UserIdAsGuid => User?.GetUserIdAsGuid();

    public string? UserName => User?.GetUserName();

    public string? Email => User?.GetEmail();

    public IEnumerable<string> Roles => User?.GetRoles() ?? Enumerable.Empty<string>();

    public IEnumerable<string> Permissions => User?.GetPermissions() ?? Enumerable.Empty<string>();

    public string? TenantId => User?.GetTenantId();

    public Guid? TenantIdAsGuid => User?.GetTenantIdAsGuid();

    public string? TenantCode => User?.GetTenantCode();

    public bool IsHost => User?.IsHost() ?? false;

    public bool IsSuperAdmin => User?.IsSuperAdmin() ?? false;

    public bool IsAdmin => User?.IsAdmin() ?? false;

    public bool HasRole(string role) => User?.HasRole(role) ?? false;

    public bool HasPermission(string permission) => User?.HasPermission(permission) ?? false;

    public ClaimsPrincipal? Principal => User;
}
