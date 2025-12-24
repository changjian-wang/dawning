using System.Security.Claims;
using Dawning.Identity.Extensions;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity;

/// <summary>
/// 当前用户接口 - 方便在服务层获取当前登录用户信息
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 是否已认证
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 用户ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// 用户ID (Guid)
    /// </summary>
    Guid? UserIdAsGuid { get; }

    /// <summary>
    /// 用户名
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 邮箱
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// 所有角色
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// 所有权限
    /// </summary>
    IEnumerable<string> Permissions { get; }

    /// <summary>
    /// 租户ID
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// 租户ID (Guid)
    /// </summary>
    Guid? TenantIdAsGuid { get; }

    /// <summary>
    /// 租户编码
    /// </summary>
    string? TenantCode { get; }

    /// <summary>
    /// 是否为宿主管理员
    /// </summary>
    bool IsHost { get; }

    /// <summary>
    /// 是否为超级管理员
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// 是否为管理员
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// 检查是否有指定角色
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// 检查是否有指定权限
    /// </summary>
    bool HasPermission(string permission);

    /// <summary>
    /// 获取原始 ClaimsPrincipal
    /// </summary>
    ClaimsPrincipal? Principal { get; }
}

/// <summary>
/// 当前用户实现
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
