namespace Dawning.Identity.Domain.Aggregates.Monitoring;

/// <summary>
/// 告警历史实体
/// </summary>
public class AlertHistory
{
    /// <summary>
    /// 告警历史ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 告警规则ID
    /// </summary>
    public long RuleId { get; set; }

    /// <summary>
    /// 规则名称(冗余)
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// 指标类型
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// 触发时的指标值
    /// </summary>
    public decimal MetricValue { get; set; }

    /// <summary>
    /// 阈值
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// 严重程度
    /// </summary>
    public string Severity { get; set; } = "warning";

    /// <summary>
    /// 告警消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 状态: triggered, acknowledged, resolved
    /// </summary>
    public string Status { get; set; } = "triggered";

    /// <summary>
    /// 触发时间
    /// </summary>
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 确认时间
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// 确认人
    /// </summary>
    public string? AcknowledgedBy { get; set; }

    /// <summary>
    /// 解决时间
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// 解决人
    /// </summary>
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// 是否已发送通知
    /// </summary>
    public bool NotifySent { get; set; }

    /// <summary>
    /// 通知结果
    /// </summary>
    public string? NotifyResult { get; set; }
}
