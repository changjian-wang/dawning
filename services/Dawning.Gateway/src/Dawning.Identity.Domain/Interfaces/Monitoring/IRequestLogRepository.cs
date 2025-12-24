using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;

namespace Dawning.Identity.Domain.Interfaces.Monitoring;

/// <summary>
/// 请求日志仓储接口
/// </summary>
public interface IRequestLogRepository
{
    /// <summary>
    /// 插入请求日志
    /// </summary>
    Task InsertAsync(RequestLog log);

    /// <summary>
    /// 分页查询请求日志
    /// </summary>
    Task<PagedData<RequestLog>> GetPagedListAsync(RequestLogQueryModel query, int page, int pageSize);

    /// <summary>
    /// 获取请求统计信息
    /// </summary>
    Task<RequestLogStatistics> GetStatisticsAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 清理过期日志
    /// </summary>
    Task<int> CleanupOldLogsAsync(DateTime cutoffDate);
}
