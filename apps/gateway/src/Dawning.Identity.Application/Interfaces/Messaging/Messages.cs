namespace Dawning.Identity.Application.Interfaces.Messaging;

/// <summary>
/// Message base class
/// </summary>
public abstract class MessageBase
{
    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Message creation time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source service
    /// </summary>
    public string? SourceService { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Correlation ID (for distributed tracing)
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Audit log message
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
/// Alert notification message
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
/// Email message
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
/// User event message
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
/// System event message
/// </summary>
public class SystemEventMessage : MessageBase
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public string? Description { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Config changed message (for distributed config synchronization)
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
/// Cache invalidation message (distributed cache sync)
/// </summary>
public class CacheInvalidationMessage : MessageBase
{
    public string CacheKey { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string Region { get; set; } = "default";
}
