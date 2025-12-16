namespace Dawning.Identity.Application.Interfaces.Monitoring;

/// <summary>
/// 告警通知服务接口
/// </summary>
public interface IAlertNotificationService
{
    /// <summary>
    /// 发送邮件通知
    /// </summary>
    Task<NotificationResult> SendEmailNotificationAsync(AlertNotificationContext context);

    /// <summary>
    /// 发送 Webhook 通知
    /// </summary>
    Task<NotificationResult> SendWebhookNotificationAsync(AlertNotificationContext context);

    /// <summary>
    /// 发送所有配置的通知
    /// </summary>
    Task<NotificationResult> SendNotificationsAsync(AlertNotificationContext context);
}

/// <summary>
/// 告警通知上下文
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
/// 通知发送结果
/// </summary>
public class NotificationResult
{
    public bool Success { get; set; }
    public string? NotificationId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, bool> ChannelResults { get; set; } = new();
}
