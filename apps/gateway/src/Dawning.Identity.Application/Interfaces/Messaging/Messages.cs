namespace Dawning.Identity.Application.Interfaces.Messaging;

/// <summary>
/// 消息基类
/// </summary>
public abstract class MessageBase
{
    /// <summary>
    /// 消息 ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 消息创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 消息来源服务
    /// </summary>
    public string? SourceService { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 关联 ID（用于分布式追踪）
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// 审计日志消息
/// </summary>
public class AuditLogMessage : MessageBase
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// 告警通知消息
/// </summary>
public class AlertNotificationMessage : MessageBase
{
    public Guid AlertRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
    public string? Description { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 邮件发送消息
/// </summary>
public class EmailMessage : MessageBase
{
    public string To { get; set; } = string.Empty;
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public int Priority { get; set; } = 0;
    public int RetryCount { get; set; } = 0;
}

/// <summary>
/// 用户事件消息
/// </summary>
public class UserEventMessage : MessageBase
{
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 系统事件消息
/// </summary>
public class SystemEventMessage : MessageBase
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public string? Description { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// 配置变更消息（用于分布式配置同步）
/// </summary>
public class ConfigChangedMessage : MessageBase
{
    public string ConfigGroup { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid ChangedBy { get; set; }
}

/// <summary>
/// 缓存失效消息（分布式缓存同步）
/// </summary>
public class CacheInvalidationMessage : MessageBase
{
    public string CacheKey { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string Region { get; set; } = "default";
}
