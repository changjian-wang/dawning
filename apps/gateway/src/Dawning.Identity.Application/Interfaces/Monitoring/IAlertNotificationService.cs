namespace Dawning.Identity.Application.Interfaces.Monitoring;

/// <summary>
/// Alert notification service interface
/// </summary>
public interface IAlertNotificationService
{
    /// <summary>
    /// Send email notification
    /// </summary>
    Task<NotificationResult> SendEmailNotificationAsync(AlertNotificationContext context);

    /// <summary>
    /// Send webhook notification
    /// </summary>
    Task<NotificationResult> SendWebhookNotificationAsync(AlertNotificationContext context);

    /// <summary>
    /// Send all configured notifications
    /// </summary>
    Task<NotificationResult> SendNotificationsAsync(AlertNotificationContext context);
}

/// <summary>
/// Alert notification context
/// </summary>
public class AlertNotificationContext
{
    public long AlertId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public decimal Threshold { get; set; }
    public string Operator { get; set; } = "gt";
    public string Severity { get; set; } = "warning";
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public List<string> NotifyChannels { get; set; } = new();
    public string? NotifyEmails { get; set; }
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// Notification send result
/// </summary>
public class NotificationResult
{
    public bool Success { get; set; }
    public string? NotificationId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, bool> ChannelResults { get; set; } = new();
}
