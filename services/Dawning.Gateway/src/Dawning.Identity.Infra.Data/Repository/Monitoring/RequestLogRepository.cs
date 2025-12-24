using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.Monitoring;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Monitoring;

/// <summary>
/// 请求日志仓储实现
/// </summary>
public class RequestLogRepository : IRequestLogRepository
{
    private readonly DbContext _context;

    public RequestLogRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 插入请求日志
    /// </summary>
    public async Task InsertAsync(RequestLog log)
    {
        var entity = log.ToEntity();
        await _context.Connection.InsertAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 分页查询请求日志
    /// </summary>
    public async Task<PagedData<RequestLog>> GetPagedListAsync(
        RequestLogQueryModel query,
        int page,
        int pageSize
    )
    {
        var builder = _context.Connection.Builder<RequestLogEntity>(_context.Transaction);

        // 应用过滤条件
        builder = builder
            .WhereIf(query.StartTime.HasValue, x => x.RequestTime >= query.StartTime!.Value)
            .WhereIf(query.EndTime.HasValue, x => x.RequestTime <= query.EndTime!.Value)
            .WhereIf(!string.IsNullOrEmpty(query.Method), x => x.Method == query.Method!.ToUpper())
            .WhereIf(!string.IsNullOrEmpty(query.Path), x => x.Path.Contains(query.Path!))
            .WhereIf(query.StatusCode.HasValue, x => x.StatusCode == query.StatusCode!.Value)
            .WhereIf(query.MinStatusCode.HasValue, x => x.StatusCode >= query.MinStatusCode!.Value)
            .WhereIf(query.MaxStatusCode.HasValue, x => x.StatusCode <= query.MaxStatusCode!.Value)
            .WhereIf(query.UserId.HasValue, x => x.UserId == query.UserId!.Value)
            .WhereIf(!string.IsNullOrEmpty(query.ClientIp), x => x.ClientIp == query.ClientIp)
            .WhereIf(query.OnlyErrors == true, x => x.StatusCode >= 400)
            .WhereIf(
                query.SlowRequestThresholdMs.HasValue,
                x => x.ResponseTimeMs >= query.SlowRequestThresholdMs!.Value
            );

        // 按请求时间降序排序
        var result = await builder
            .OrderByDescending(x => x.RequestTime)
            .AsPagedListAsync(page, pageSize);

        return new PagedData<RequestLog>
        {
            PageIndex = page,
            PageSize = pageSize,
            TotalCount = result.TotalItems,
            Items = result.Values.ToModels(),
        };
    }

    /// <summary>
    /// 获取请求统计信息
    /// </summary>
    public async Task<RequestLogStatistics> GetStatisticsAsync(DateTime startTime, DateTime endTime)
    {
        var statistics = new RequestLogStatistics { StartTime = startTime, EndTime = endTime };

        // 获取基础统计
        var builder = _context
            .Connection.Builder<RequestLogEntity>(_context.Transaction)
            .Where(x => x.RequestTime >= startTime)
            .Where(x => x.RequestTime <= endTime);

        var allLogs = await builder.AsListAsync();

        if (!allLogs.Any())
        {
            return statistics;
        }

        statistics.TotalRequests = allLogs.Count();
        statistics.SuccessRequests = allLogs.Count(x => x.StatusCode >= 200 && x.StatusCode < 300);
        statistics.ClientErrors = allLogs.Count(x => x.StatusCode >= 400 && x.StatusCode < 500);
        statistics.ServerErrors = allLogs.Count(x => x.StatusCode >= 500);
        statistics.AverageResponseTimeMs = allLogs.Average(x => x.ResponseTimeMs);
        statistics.MaxResponseTimeMs = allLogs.Max(x => x.ResponseTimeMs);
        statistics.MinResponseTimeMs = allLogs.Min(x => x.ResponseTimeMs);

        // 状态码分布
        statistics.StatusCodeDistribution = allLogs
            .GroupBy(x => x.StatusCode)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        // Top 10 路径统计
        statistics.TopPaths = allLogs
            .GroupBy(x => x.Path)
            .Select(g => new PathStatisticModel
            {
                Path = g.Key,
                RequestCount = g.Count(),
                AverageResponseTimeMs = g.Average(x => x.ResponseTimeMs),
                ErrorCount = g.Count(x => x.StatusCode >= 400),
            })
            .OrderByDescending(x => x.RequestCount)
            .Take(10)
            .ToList();

        // 按小时统计请求数
        statistics.HourlyRequests = allLogs
            .GroupBy(x => x.RequestTime.ToString("yyyy-MM-dd HH:00"))
            .ToDictionary(g => g.Key, g => (long)g.Count());

        // P95 和 P99
        var sortedResponseTimes = allLogs.Select(x => x.ResponseTimeMs).OrderBy(x => x).ToList();
        if (sortedResponseTimes.Count > 0)
        {
            var p95Index = (int)Math.Ceiling(sortedResponseTimes.Count * 0.95) - 1;
            var p99Index = (int)Math.Ceiling(sortedResponseTimes.Count * 0.99) - 1;

            statistics.P95ResponseTimeMs = sortedResponseTimes[
                Math.Max(0, Math.Min(p95Index, sortedResponseTimes.Count - 1))
            ];
            statistics.P99ResponseTimeMs = sortedResponseTimes[
                Math.Max(0, Math.Min(p99Index, sortedResponseTimes.Count - 1))
            ];
        }

        return statistics;
    }

    /// <summary>
    /// 清理过期日志
    /// </summary>
    public async Task<int> CleanupOldLogsAsync(DateTime cutoffDate)
    {
        var logsToDelete = await _context
            .Connection.Builder<RequestLogEntity>(_context.Transaction)
            .Where(x => x.RequestTime < cutoffDate)
            .AsListAsync();

        var count = 0;
        foreach (var log in logsToDelete)
        {
            var deleted = await _context.Connection.DeleteAsync(log, _context.Transaction);
            if (deleted)
                count++;
        }

        return count;
    }
}
