using Dawning.Identity.Domain.Core.Events;

namespace Dawning.Identity.Application.Dtos.IntegrationEvents;

/// <summary>
/// Audit Log Integration Event (sent to Kafka for asynchronous persistence)
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
/// Alert Notification Integration Event (sent to Kafka for broadcast notifications)
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
/// Email Integration Event (sent to Kafka email queue)
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
/// Configuration Changed Integration Event (for multi-instance configuration sync)
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
/// Cache Invalidation Integration Event (for distributed cache consistency)
/// </summary>
public class CacheInvalidationIntegrationEvent : IntegrationEvent
{
    public string CacheKey { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string? Region { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// User Event Integration Event (cross-service user behavior notification)
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
/// System Event Integration Event
/// </summary>
public class SystemEventIntegrationEvent : IntegrationEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string? Source { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
