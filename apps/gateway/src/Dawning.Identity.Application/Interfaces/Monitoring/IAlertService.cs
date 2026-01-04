using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Application.Interfaces.Monitoring;

/// <summary>
/// Alert service interface
/// </summary>
public interface IAlertService
{
    #region Alert Rule Management

    /// <summary>
    /// Get all alert rules
    /// </summary>
    Task<IEnumerable<AlertRuleDto>> GetAllRulesAsync();

    /// <summary>
    /// Get enabled alert rules
    /// </summary>
    Task<IEnumerable<AlertRuleDto>> GetEnabledRulesAsync();

    /// <summary>
    /// Get alert rule by ID
    /// </summary>
    Task<AlertRuleDto?> GetRuleByIdAsync(long id);

    /// <summary>
    /// Create alert rule
    /// </summary>
    Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleRequest request);

    /// <summary>
    /// Update alert rule
    /// </summary>
    Task<AlertRuleDto?> UpdateRuleAsync(long id, UpdateAlertRuleRequest request);

    /// <summary>
    /// Delete alert rule
    /// </summary>
    Task<bool> DeleteRuleAsync(long id);

    /// <summary>
    /// Enable/disable alert rule
    /// </summary>
    Task<bool> SetRuleEnabledAsync(long id, bool isEnabled);

    #endregion

    #region Alert History Management

    /// <summary>
    /// Get alert history list (paginated)
    /// </summary>
    Task<PagedData<AlertHistoryDto>> GetAlertHistoryAsync(AlertHistoryQueryParams queryParams);

    /// <summary>
    /// Get alert history by ID
    /// </summary>
    Task<AlertHistoryDto?> GetAlertHistoryByIdAsync(long id);

    /// <summary>
    /// Update alert status (acknowledge/resolve)
    /// </summary>
    Task<bool> UpdateAlertStatusAsync(long id, UpdateAlertStatusRequest request);

    /// <summary>
    /// Get unresolved alerts list
    /// </summary>
    Task<IEnumerable<AlertHistoryDto>> GetUnresolvedAlertsAsync();

    #endregion

    #region Alert Check and Trigger

    /// <summary>
    /// Check all enabled alert rules
    /// </summary>
    Task CheckAlertsAsync();

    /// <summary>
    /// Manually trigger alert check
    /// </summary>
    Task<AlertCheckResult> TriggerAlertCheckAsync();

    /// <summary>
    /// Get alert statistics
    /// </summary>
    Task<AlertStatisticsDto> GetAlertStatisticsAsync();

    #endregion
}

/// <summary>
/// Alert check result
/// </summary>
public class AlertCheckResult
{
    public int RulesChecked { get; set; }
    public int AlertsTriggered { get; set; }
    public int NotificationsSent { get; set; }
    public List<string> TriggeredAlerts { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
