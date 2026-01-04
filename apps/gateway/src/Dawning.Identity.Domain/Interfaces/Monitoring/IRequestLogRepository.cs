using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;

namespace Dawning.Identity.Domain.Interfaces.Monitoring;

/// <summary>
/// Request log repository interface
/// </summary>
public interface IRequestLogRepository
{
    /// <summary>
    /// Insert request log
    /// </summary>
    Task InsertAsync(RequestLog log);

    /// <summary>
    /// Paginated query for request logs
    /// </summary>
    Task<PagedData<RequestLog>> GetPagedListAsync(
        RequestLogQueryModel query,
        int page,
        int pageSize
    );

    /// <summary>
    /// Get request statistics
    /// </summary>
    Task<RequestLogStatistics> GetStatisticsAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Cleanup expired logs
    /// </summary>
    Task<int> CleanupOldLogsAsync(DateTime cutoffDate);
}
