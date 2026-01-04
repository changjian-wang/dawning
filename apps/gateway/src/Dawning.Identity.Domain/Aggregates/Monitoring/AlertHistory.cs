namespace Dawning.Identity.Domain.Aggregates.Monitoring;

/// <summary>
/// Alert History Entity
/// </summary>
public class AlertHistory
{
    /// <summary>
    /// Alert history ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Alert rule ID
    /// </summary>
    public long RuleId { get; set; }

    /// <summary>
    /// Rule name (denormalized)
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Metric type
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Metric value when triggered
    /// </summary>
    public decimal MetricValue { get; set; }

    /// <summary>
    /// Threshold
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// Severity
    /// </summary>
    public string Severity { get; set; } = "warning";

    /// <summary>
    /// Alert message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Status: triggered, acknowledged, resolved
    /// </summary>
    public string Status { get; set; } = "triggered";

    /// <summary>
    /// Triggered time
    /// </summary>
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Acknowledged time
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Acknowledged by
    /// </summary>
    public string? AcknowledgedBy { get; set; }

    /// <summary>
    /// Resolved time
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Resolved by
    /// </summary>
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// Whether notification was sent
    /// </summary>
    public bool NotifySent { get; set; }

    /// <summary>
    /// Notification result
    /// </summary>
    public string? NotifyResult { get; set; }
}
