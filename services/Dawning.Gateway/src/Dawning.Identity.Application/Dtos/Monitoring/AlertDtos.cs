namespace Dawning.Identity.Application.Dtos.Monitoring;

/// <summary>
/// 告警规则 DTO
/// </summary>
public class AlertRuleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MetricType { get; set; } = string.Empty;
    public string MetricTypeDisplay => GetMetricTypeDisplay(MetricType);
    public string Operator { get; set; } = "gt";
    public string OperatorDisplay => GetOperatorDisplay(Operator);
    public decimal Threshold { get; set; }
    public int DurationSeconds { get; set; }
    public string Severity { get; set; } = "warning";
    public bool IsEnabled { get; set; }
    public List<string> NotifyChannels { get; set; } = new();
    public string? NotifyEmails { get; set; }
    public string? WebhookUrl { get; set; }
    public int CooldownMinutes { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    private static string GetMetricTypeDisplay(string metricType) =>
        metricType switch
        {
            "cpu" => "CPU 使用率",
            "memory" => "内存使用率",
            "response_time" => "响应时间",
            "error_rate" => "错误率",
            "request_count" => "请求数量",
            _ => metricType,
        };

    private static string GetOperatorDisplay(string op) =>
        op switch
        {
            "gt" => ">",
            "gte" => ">=",
            "lt" => "<",
            "lte" => "<=",
            "eq" => "=",
            _ => op,
        };
}

/// <summary>
/// 创建告警规则请求
/// </summary>
public class CreateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MetricType { get; set; } = string.Empty;
    public string Operator { get; set; } = "gt";
    public decimal Threshold { get; set; }
    public int DurationSeconds { get; set; } = 60;
    public string Severity { get; set; } = "warning";
    public bool IsEnabled { get; set; } = true;
    public List<string> NotifyChannels { get; set; } = new();
    public string? NotifyEmails { get; set; }
    public string? WebhookUrl { get; set; }
    public int CooldownMinutes { get; set; } = 5;
}

/// <summary>
/// 更新告警规则请求
/// </summary>
public class UpdateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MetricType { get; set; } = string.Empty;
    public string Operator { get; set; } = "gt";
    public decimal Threshold { get; set; }
    public int DurationSeconds { get; set; } = 60;
    public string Severity { get; set; } = "warning";
    public bool IsEnabled { get; set; } = true;
    public List<string> NotifyChannels { get; set; } = new();
    public string? NotifyEmails { get; set; }
    public string? WebhookUrl { get; set; }
    public int CooldownMinutes { get; set; } = 5;
}

/// <summary>
/// 告警历史 DTO
/// </summary>
public class AlertHistoryDto
{
    public long Id { get; set; }
    public long RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public string MetricTypeDisplay => GetMetricTypeDisplay(MetricType);
    public decimal MetricValue { get; set; }
    public decimal Threshold { get; set; }
    public string Severity { get; set; } = "warning";
    public string? Message { get; set; }
    public string Status { get; set; } = "triggered";
    public string StatusDisplay => GetStatusDisplay(Status);
    public DateTime TriggeredAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public bool NotifySent { get; set; }

    private static string GetMetricTypeDisplay(string metricType) =>
        metricType switch
        {
            "cpu" => "CPU 使用率",
            "memory" => "内存使用率",
            "response_time" => "响应时间",
            "error_rate" => "错误率",
            "request_count" => "请求数量",
            _ => metricType,
        };

    private static string GetStatusDisplay(string status) =>
        status switch
        {
            "triggered" => "已触发",
            "acknowledged" => "已确认",
            "resolved" => "已解决",
            _ => status,
        };
}

/// <summary>
/// 告警历史查询参数
/// </summary>
public class AlertHistoryQueryParams
{
    public long? RuleId { get; set; }
    public string? MetricType { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 告警统计 DTO
/// </summary>
public class AlertStatisticsDto
{
    public int TotalRules { get; set; }
    public int EnabledRules { get; set; }
    public int TotalAlertsToday { get; set; }
    public int UnresolvedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int WarningAlerts { get; set; }
    public Dictionary<string, int> AlertsByMetricType { get; set; } = new();
    public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
}

/// <summary>
/// 确认/解决告警请求
/// </summary>
public class UpdateAlertStatusRequest
{
    public string Status { get; set; } = "acknowledged";
    public string? ResolvedBy { get; set; }
}
