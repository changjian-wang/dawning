using Dawning.Identity.Domain.Aggregates.Monitoring;

namespace Dawning.Identity.Domain.Interfaces.Monitoring;

/// <summary>
/// 告警规则仓储接口
/// </summary>
public interface IAlertRuleRepository
{
    /// <summary>
    /// 获取所有告警规则
    /// </summary>
    Task<IEnumerable<AlertRule>> GetAllAsync();

    /// <summary>
    /// 获取启用的告警规则
    /// </summary>
    Task<IEnumerable<AlertRule>> GetEnabledAsync();

    /// <summary>
    /// 根据ID获取告警规则
    /// </summary>
    Task<AlertRule?> GetByIdAsync(long id);

    /// <summary>
    /// 创建告警规则
    /// </summary>
    Task<long> CreateAsync(AlertRule rule);

    /// <summary>
    /// 更新告警规则
    /// </summary>
    Task<bool> UpdateAsync(AlertRule rule);

    /// <summary>
    /// 删除告警规则
    /// </summary>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 更新启用状态
    /// </summary>
    Task<bool> SetEnabledAsync(long id, bool isEnabled);

    /// <summary>
    /// 更新最后触发时间
    /// </summary>
    Task<bool> UpdateLastTriggeredAsync(long id, DateTime triggeredAt);
}
