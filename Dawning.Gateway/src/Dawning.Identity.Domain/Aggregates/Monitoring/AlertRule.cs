namespace Dawning.Identity.Domain.Aggregates.Monitoring;

/// <summary>
/// 告警规则聚合根
/// </summary>
public class AlertRule
{
    /// <summary>
    /// 规则ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 规则名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 规则描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 指标类型: cpu, memory, response_time, error_rate, request_count
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// 比较操作符: gt, gte, lt, lte, eq
    /// </summary>
    public string Operator { get; set; } = "gt";

    /// <summary>
    /// 阈值
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// 持续时间(秒)，超过此时间才触发告警
    /// </summary>
    public int DurationSeconds { get; set; } = 60;

    /// <summary>
    /// 严重程度: info, warning, error, critical
    /// </summary>
    public string Severity { get; set; } = "warning";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 通知渠道，JSON数组: ["email", "webhook"]
    /// </summary>
    public string? NotifyChannels { get; set; }

    /// <summary>
    /// 通知邮箱，逗号分隔
    /// </summary>
    public string? NotifyEmails { get; set; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// 冷却时间(分钟)，避免重复告警
    /// </summary>
    public int CooldownMinutes { get; set; } = 5;

    /// <summary>
    /// 上次触发时间
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
