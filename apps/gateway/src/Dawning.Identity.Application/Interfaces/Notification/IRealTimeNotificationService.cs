namespace Dawning.Identity.Application.Interfaces.Notification;

/// <summary>
/// Real-time push service interface
/// Used for pushing alerts, system messages, etc. to users in real-time
/// </summary>
public interface IRealTimeNotificationService
{
    /// <summary>
    /// Push alert notification
    /// </summary>
    Task SendAlertNotificationAsync(RealTimeAlertNotification alert);

    /// <summary>
    /// Push system message
    /// </summary>
    Task SendSystemMessageAsync(RealTimeSystemMessage message);

    /// <summary>
    /// Push log entry (real-time log stream)
    /// </summary>
    Task SendLogEntryAsync(RealTimeLogEntry logEntry);

    /// <summary>
    /// Push notification to specific user
    /// </summary>
    Task SendToUserAsync(Guid userId, RealTimeNotification notification);

    /// <summary>
    /// Push notification to specific role
    /// </summary>
    Task SendToRoleAsync(string role, RealTimeNotification notification);
}

/// <summary>
/// Real-time notification base class
/// </summary>
public class RealTimeNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Real-time alert notification
/// </summary>
public class RealTimeAlertNotification : RealTimeNotification
{
    public string Severity { get; set; } = "warning";
    public long? RuleId { get; set; }
    public string? RuleName { get; set; }
    public string? MetricType { get; set; }
    public decimal? Value { get; set; }
    public decimal? Threshold { get; set; }
}

/// <summary>
/// Real-time system message
/// </summary>
public class RealTimeSystemMessage : RealTimeNotification
{
    public bool RequireAcknowledge { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Real-time log entry
/// </summary>
public class RealTimeLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
}
