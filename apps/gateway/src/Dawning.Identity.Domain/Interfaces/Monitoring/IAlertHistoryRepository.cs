using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;

namespace Dawning.Identity.Domain.Interfaces.Monitoring;

/// <summary>
/// Alert history repository interface
/// </summary>
public interface IAlertHistoryRepository
{
    /// <summary>
    /// Paginated query for alert history
    /// </summary>
    Task<PagedData<AlertHistory>> GetPagedListAsync(
        AlertHistoryQueryModel model,
        int page,
        int pageSize
    );

    /// <summary>
    /// Get alert history by ID
    /// </summary>
    Task<AlertHistory?> GetByIdAsync(long id);

    /// <summary>
    /// Create alert history
    /// </summary>
    Task<long> CreateAsync(AlertHistory history);

    /// <summary>
    /// Update alert status to acknowledged
    /// </summary>
    Task<bool> AcknowledgeAsync(long id, string acknowledgedBy);

    /// <summary>
    /// Update alert status to resolved
    /// </summary>
    Task<bool> ResolveAsync(long id, string resolvedBy);

    /// <summary>
    /// Update alert status
    /// </summary>
    Task<bool> UpdateStatusAsync(long id, string status);

    /// <summary>
    /// Get unresolved alerts
    /// </summary>
    Task<IEnumerable<AlertHistory>> GetUnresolvedAsync();

    /// <summary>
    /// Update notification send status
    /// </summary>
    Task<bool> UpdateNotifyResultAsync(long id, bool sent, string? result);

    /// <summary>
    /// Get alert statistics
    /// </summary>
    Task<AlertStatistics> GetStatisticsAsync();
}
