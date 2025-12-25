using Dawning.Identity.Domain.Core.Events;

namespace Dawning.Identity.Domain.Events;

#region User Events (用户相关事件)

/// <summary>
/// 用户创建事件
/// </summary>
public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string? Email { get; }

    public UserCreatedEvent(Guid userId, string userName, string? email)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
    }
}

/// <summary>
/// 用户删除事件
/// </summary>
public class UserDeletedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }

    public UserDeletedEvent(Guid userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}

/// <summary>
/// 用户密码变更事件
/// </summary>
public class UserPasswordChangedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string? IpAddress { get; }

    public UserPasswordChangedEvent(Guid userId, string userName, string? ipAddress = null)
    {
        UserId = userId;
        UserName = userName;
        IpAddress = ipAddress;
    }
}

/// <summary>
/// 用户登录事件
/// </summary>
public class UserLoggedInEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public bool Success { get; }
    public string? FailureReason { get; }

    public UserLoggedInEvent(
        Guid userId,
        string userName,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? failureReason = null
    )
    {
        UserId = userId;
        UserName = userName;
        Success = success;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        FailureReason = failureReason;
    }
}

/// <summary>
/// 用户登出事件
/// </summary>
public class UserLoggedOutEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }

    public UserLoggedOutEvent(Guid userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}

/// <summary>
/// 用户锁定事件
/// </summary>
public class UserLockedOutEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public DateTime LockoutEnd { get; }
    public string Reason { get; }

    public UserLockedOutEvent(Guid userId, string userName, DateTime lockoutEnd, string reason)
    {
        UserId = userId;
        UserName = userName;
        LockoutEnd = lockoutEnd;
        Reason = reason;
    }
}

#endregion

#region Role Events (角色相关事件)

/// <summary>
/// 角色分配事件
/// </summary>
public class RoleAssignedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public Guid RoleId { get; }
    public string RoleName { get; }

    public RoleAssignedEvent(Guid userId, string userName, Guid roleId, string roleName)
    {
        UserId = userId;
        UserName = userName;
        RoleId = roleId;
        RoleName = roleName;
    }
}

/// <summary>
/// 角色移除事件
/// </summary>
public class RoleRevokedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public Guid RoleId { get; }
    public string RoleName { get; }

    public RoleRevokedEvent(Guid userId, string userName, Guid roleId, string roleName)
    {
        UserId = userId;
        UserName = userName;
        RoleId = roleId;
        RoleName = roleName;
    }
}

/// <summary>
/// 权限变更事件
/// </summary>
public class PermissionChangedEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public List<string> AddedPermissions { get; }
    public List<string> RemovedPermissions { get; }

    public PermissionChangedEvent(
        Guid roleId,
        string roleName,
        List<string>? addedPermissions = null,
        List<string>? removedPermissions = null
    )
    {
        RoleId = roleId;
        RoleName = roleName;
        AddedPermissions = addedPermissions ?? new List<string>();
        RemovedPermissions = removedPermissions ?? new List<string>();
    }
}

#endregion

#region Configuration Events (配置相关事件)

/// <summary>
/// 系统配置变更事件
/// </summary>
public class ConfigurationChangedEvent : DomainEvent
{
    public string ConfigGroup { get; }
    public string ConfigKey { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }
    public Guid? ChangedBy { get; }

    public ConfigurationChangedEvent(
        string configGroup,
        string configKey,
        string? oldValue,
        string? newValue,
        Guid? changedBy = null
    )
    {
        ConfigGroup = configGroup;
        ConfigKey = configKey;
        OldValue = oldValue;
        NewValue = newValue;
        ChangedBy = changedBy;
    }
}

#endregion

#region Alert Events (告警相关事件)

/// <summary>
/// 告警触发事件
/// </summary>
public class AlertTriggeredEvent : DomainEvent
{
    public Guid AlertRuleId { get; }
    public string RuleName { get; }
    public string MetricType { get; }
    public string Severity { get; }
    public double CurrentValue { get; }
    public double Threshold { get; }

    public AlertTriggeredEvent(
        Guid alertRuleId,
        string ruleName,
        string metricType,
        string severity,
        double currentValue,
        double threshold
    )
    {
        AlertRuleId = alertRuleId;
        RuleName = ruleName;
        MetricType = metricType;
        Severity = severity;
        CurrentValue = currentValue;
        Threshold = threshold;
    }
}

/// <summary>
/// 告警恢复事件
/// </summary>
public class AlertResolvedEvent : DomainEvent
{
    public Guid AlertRuleId { get; }
    public string RuleName { get; }
    public TimeSpan Duration { get; }

    public AlertResolvedEvent(Guid alertRuleId, string ruleName, TimeSpan duration)
    {
        AlertRuleId = alertRuleId;
        RuleName = ruleName;
        Duration = duration;
    }
}

#endregion

#region Audit Events (审计相关事件)

/// <summary>
/// 实体变更事件（通用审计）
/// </summary>
public class EntityChangedEvent : DomainEvent
{
    public string EntityType { get; }
    public Guid EntityId { get; }
    public string Action { get; }
    public Guid? UserId { get; }
    public string? UserName { get; }
    public string? OldValues { get; }
    public string? NewValues { get; }

    public EntityChangedEvent(
        string entityType,
        Guid entityId,
        string action,
        Guid? userId = null,
        string? userName = null,
        string? oldValues = null,
        string? newValues = null
    )
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        UserId = userId;
        UserName = userName;
        OldValues = oldValues;
        NewValues = newValues;
    }
}

#endregion
