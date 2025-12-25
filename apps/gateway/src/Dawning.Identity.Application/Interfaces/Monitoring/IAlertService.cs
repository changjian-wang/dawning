using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Application.Interfaces.Monitoring;

/// <summary>
/// 告警服务接口
/// </summary>
public interface IAlertService
{
    #region 告警规则管理

    /// <summary>
    /// 获取所有告警规则
    /// </summary>
    Task<IEnumerable<AlertRuleDto>> GetAllRulesAsync();

    /// <summary>
    /// 获取启用的告警规则
    /// </summary>
    Task<IEnumerable<AlertRuleDto>> GetEnabledRulesAsync();

    /// <summary>
    /// 根据ID获取告警规则
    /// </summary>
    Task<AlertRuleDto?> GetRuleByIdAsync(long id);

    /// <summary>
    /// 创建告警规则
    /// </summary>
    Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleRequest request);

    /// <summary>
    /// 更新告警规则
    /// </summary>
    Task<AlertRuleDto?> UpdateRuleAsync(long id, UpdateAlertRuleRequest request);

    /// <summary>
    /// 删除告警规则
    /// </summary>
    Task<bool> DeleteRuleAsync(long id);

    /// <summary>
    /// 启用/禁用告警规则
    /// </summary>
    Task<bool> SetRuleEnabledAsync(long id, bool isEnabled);

    #endregion

    #region 告警历史管理

    /// <summary>
    /// 获取告警历史列表（分页）
    /// </summary>
    Task<PagedData<AlertHistoryDto>> GetAlertHistoryAsync(AlertHistoryQueryParams queryParams);

    /// <summary>
    /// 根据ID获取告警历史
    /// </summary>
    Task<AlertHistoryDto?> GetAlertHistoryByIdAsync(long id);

    /// <summary>
    /// 更新告警状态（确认/解决）
    /// </summary>
    Task<bool> UpdateAlertStatusAsync(long id, UpdateAlertStatusRequest request);

    /// <summary>
    /// 获取未解决的告警列表
    /// </summary>
    Task<IEnumerable<AlertHistoryDto>> GetUnresolvedAlertsAsync();

    #endregion

    #region 告警检查与触发

    /// <summary>
    /// 检查所有启用的告警规则
    /// </summary>
    Task CheckAlertsAsync();

    /// <summary>
    /// 手动触发告警检查
    /// </summary>
    Task<AlertCheckResult> TriggerAlertCheckAsync();

    /// <summary>
    /// 获取告警统计
    /// </summary>
    Task<AlertStatisticsDto> GetAlertStatisticsAsync();

    #endregion
}

/// <summary>
/// 告警检查结果
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
