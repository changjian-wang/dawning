using Dawning.Identity.Domain.Aggregates.Monitoring;

namespace Dawning.Identity.Domain.Interfaces.Monitoring;

/// <summary>
/// Alert rule repository interface
/// </summary>
public interface IAlertRuleRepository
{
    /// <summary>
    /// Get all alert rules
    /// </summary>
    Task<IEnumerable<AlertRule>> GetAllAsync();

    /// <summary>
    /// Get enabled alert rules
    /// </summary>
    Task<IEnumerable<AlertRule>> GetEnabledAsync();

    /// <summary>
    /// Get alert rule by ID
    /// </summary>
    Task<AlertRule?> GetByIdAsync(long id);

    /// <summary>
    /// Create alert rule
    /// </summary>
    Task<long> CreateAsync(AlertRule rule);

    /// <summary>
    /// Update alert rule
    /// </summary>
    Task<bool> UpdateAsync(AlertRule rule);

    /// <summary>
    /// Delete alert rule
    /// </summary>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// Update enabled status
    /// </summary>
    Task<bool> SetEnabledAsync(long id, bool isEnabled);

    /// <summary>
    /// Update last triggered time
    /// </summary>
    Task<bool> UpdateLastTriggeredAsync(long id, DateTime triggeredAt);
}
