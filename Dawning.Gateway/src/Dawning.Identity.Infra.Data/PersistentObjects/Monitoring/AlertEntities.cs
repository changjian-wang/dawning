using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Monitoring
{
    /// <summary>
    /// 告警规则实体
    /// </summary>
    [Table("alert_rules")]
    public class AlertRuleEntity
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 规则描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 指标类型: cpu, memory, response_time, error_rate, request_count
        /// </summary>
        [Column("metric_type")]
        public string MetricType { get; set; } = string.Empty;

        /// <summary>
        /// 比较操作符: gt, gte, lt, lte, eq
        /// </summary>
        [Column("operator")]
        public string Operator { get; set; } = "gt";

        /// <summary>
        /// 阈值
        /// </summary>
        [Column("threshold")]
        public decimal Threshold { get; set; }

        /// <summary>
        /// 持续时间(秒)，超过此时间才触发告警
        /// </summary>
        [Column("duration_seconds")]
        public int DurationSeconds { get; set; } = 60;

        /// <summary>
        /// 严重程度: info, warning, error, critical
        /// </summary>
        [Column("severity")]
        public string Severity { get; set; } = "warning";

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 通知渠道，JSON数组: ["email", "webhook"]
        /// </summary>
        [Column("notify_channels")]
        public string? NotifyChannels { get; set; }

        /// <summary>
        /// 通知邮箱，逗号分隔
        /// </summary>
        [Column("notify_emails")]
        public string? NotifyEmails { get; set; }

        /// <summary>
        /// Webhook URL
        /// </summary>
        [Column("webhook_url")]
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// 冷却时间(分钟)，避免重复告警
        /// </summary>
        [Column("cooldown_minutes")]
        public int CooldownMinutes { get; set; } = 5;

        /// <summary>
        /// 上次触发时间
        /// </summary>
        [Column("last_triggered_at")]
        public DateTime? LastTriggeredAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 告警历史实体
    /// </summary>
    [Table("alert_history")]
    public class AlertHistoryEntity
    {
        /// <summary>
        /// 告警历史ID
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// 告警规则ID
        /// </summary>
        [Column("rule_id")]
        public long RuleId { get; set; }

        /// <summary>
        /// 规则名称(冗余)
        /// </summary>
        [Column("rule_name")]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// 指标类型
        /// </summary>
        [Column("metric_type")]
        public string MetricType { get; set; } = string.Empty;

        /// <summary>
        /// 触发时的指标值
        /// </summary>
        [Column("metric_value")]
        public decimal MetricValue { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        [Column("threshold")]
        public decimal Threshold { get; set; }

        /// <summary>
        /// 严重程度
        /// </summary>
        [Column("severity")]
        public string Severity { get; set; } = "warning";

        /// <summary>
        /// 告警消息
        /// </summary>
        [Column("message")]
        public string? Message { get; set; }

        /// <summary>
        /// 状态: triggered, acknowledged, resolved
        /// </summary>
        [Column("status")]
        public string Status { get; set; } = "triggered";

        /// <summary>
        /// 触发时间
        /// </summary>
        [Column("triggered_at")]
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 确认时间
        /// </summary>
        [Column("acknowledged_at")]
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// 确认人
        /// </summary>
        [Column("acknowledged_by")]
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// 解决时间
        /// </summary>
        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// 解决人
        /// </summary>
        [Column("resolved_by")]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// 是否已发送通知
        /// </summary>
        [Column("notify_sent")]
        public bool NotifySent { get; set; }

        /// <summary>
        /// 通知结果
        /// </summary>
        [Column("notify_result")]
        public string? NotifyResult { get; set; }
    }
}
