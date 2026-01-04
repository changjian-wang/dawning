namespace Dawning.Identity.Domain.Aggregates.Monitoring;

/// <summary>
/// Alert Rule Aggregate Root
/// </summary>
public class AlertRule
{
    /// <summary>
    /// Rule ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Rule name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Rule description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Metric type: cpu, memory, response_time, error_rate, request_count
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Comparison operator: gt, gte, lt, lte, eq
    /// </summary>
    public string Operator { get; set; } = "gt";

    /// <summary>
    /// Threshold
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// Duration (seconds), alert is triggered only after this time is exceeded
    /// </summary>
    public int DurationSeconds { get; set; } = 60;

    /// <summary>
    /// Severity: info, warning, error, critical
    /// </summary>
    public string Severity { get; set; } = "warning";

    /// <summary>
    /// Whether enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Notification channels, JSON array: ["email", "webhook"]
    /// </summary>
    public string? NotifyChannels { get; set; }

    /// <summary>
    /// Notification emails, comma-separated
    /// </summary>
    public string? NotifyEmails { get; set; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Cooldown time (minutes), to avoid repeated alerts
    /// </summary>
    public int CooldownMinutes { get; set; } = 5;

    /// <summary>
    /// Last triggered time
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Created time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated time
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
