namespace Dawning.Identity.Domain.Models.Monitoring;

/// <summary>
/// Alert history query model
/// </summary>
public class AlertHistoryQueryModel
{
    public long? RuleId { get; set; }
    public string? MetricType { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Alert statistics
/// </summary>
public class AlertStatistics
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
