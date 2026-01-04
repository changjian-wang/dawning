using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Monitoring
{
    /// <summary>
    /// Alert rule entity
    /// </summary>
    [Table("alert_rules")]
    public class AlertRuleEntity
    {
        /// <summary>
        /// Rule ID
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Rule name
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Rule description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Metric type: cpu, memory, response_time, error_rate, request_count
        /// </summary>
        [Column("metric_type")]
        public string MetricType { get; set; } = string.Empty;

        /// <summary>
        /// Comparison operator: gt, gte, lt, lte, eq
        /// </summary>
        [Column("operator")]
        public string Operator { get; set; } = "gt";

        /// <summary>
        /// Threshold
        /// </summary>
        [Column("threshold")]
        public decimal Threshold { get; set; }

        /// <summary>
        /// Duration in seconds, alert triggers only after exceeding this time
        /// </summary>
        [Column("duration_seconds")]
        public int DurationSeconds { get; set; } = 60;

        /// <summary>
        /// Severity: info, warning, error, critical
        /// </summary>
        [Column("severity")]
        public string Severity { get; set; } = "warning";

        /// <summary>
        /// Whether enabled
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Notification channels, JSON array: ["email", "webhook"]
        /// </summary>
        [Column("notify_channels")]
        public string? NotifyChannels { get; set; }

        /// <summary>
        /// Notification emails, comma separated
        /// </summary>
        [Column("notify_emails")]
        public string? NotifyEmails { get; set; }

        /// <summary>
        /// Webhook URL
        /// </summary>
        [Column("webhook_url")]
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Cooldown time in minutes, to avoid repeated alerts
        /// </summary>
        [Column("cooldown_minutes")]
        public int CooldownMinutes { get; set; } = 5;

        /// <summary>
        /// Last triggered time
        /// </summary>
        [Column("last_triggered_at")]
        public DateTime? LastTriggeredAt { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Alert history entity
    /// </summary>
    [Table("alert_history")]
    public class AlertHistoryEntity
    {
        /// <summary>
        /// Alert history ID
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Alert rule ID
        /// </summary>
        [Column("rule_id")]
        public long RuleId { get; set; }

        /// <summary>
        /// Rule name (redundant)
        /// </summary>
        [Column("rule_name")]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Metric type
        /// </summary>
        [Column("metric_type")]
        public string MetricType { get; set; } = string.Empty;

        /// <summary>
        /// Metric value at trigger time
        /// </summary>
        [Column("metric_value")]
        public decimal MetricValue { get; set; }

        /// <summary>
        /// Threshold
        /// </summary>
        [Column("threshold")]
        public decimal Threshold { get; set; }

        /// <summary>
        /// Severity
        /// </summary>
        [Column("severity")]
        public string Severity { get; set; } = "warning";

        /// <summary>
        /// Alert message
        /// </summary>
        [Column("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Status: triggered, acknowledged, resolved
        /// </summary>
        [Column("status")]
        public string Status { get; set; } = "triggered";

        /// <summary>
        /// Triggered time
        /// </summary>
        [Column("triggered_at")]
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Acknowledged time
        /// </summary>
        [Column("acknowledged_at")]
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// Acknowledged by
        /// </summary>
        [Column("acknowledged_by")]
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// Resolved time
        /// </summary>
        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Resolved by
        /// </summary>
        [Column("resolved_by")]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// Whether notification has been sent
        /// </summary>
        [Column("notify_sent")]
        public bool NotifySent { get; set; }

        /// <summary>
        /// Notification result
        /// </summary>
        [Column("notify_result")]
        public string? NotifyResult { get; set; }
    }
}
