using Dawning.Identity.Domain.Core.Events;

namespace Dawning.Identity.Application.Dtos.IntegrationEvents;

/// <summary>
/// 审计日志集成事件（发送到 Kafka 用于异步持久化）
/// </summary>
public class AuditLogIntegrationEvent : IntegrationEvent
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
}

/// <summary>
/// 告警通知集成事件（发送到 Kafka 用于广播通知）
/// </summary>
public class AlertNotificationIntegrationEvent : IntegrationEvent
{
    public Guid AlertRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public string Severity { get; set; } = "warning";
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// 邮件发送集成事件（发送到 Kafka 邮件队列）
/// </summary>
public class EmailIntegrationEvent : IntegrationEvent
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
/// 配置变更集成事件（用于多实例配置同步）
/// </summary>
public class ConfigChangedIntegrationEvent : IntegrationEvent
{
    public string ConfigGroup { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
}

/// <summary>
/// 缓存失效集成事件（用于分布式缓存一致性）
/// </summary>
public class CacheInvalidationIntegrationEvent : IntegrationEvent
{
    public string CacheKey { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string? Region { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// 用户事件集成事件（跨服务用户行为通知）
/// </summary>
public class UserEventIntegrationEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Dictionary<string, object>? EventData { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 系统事件集成事件
/// </summary>
public class SystemEventIntegrationEvent : IntegrationEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string? Source { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
