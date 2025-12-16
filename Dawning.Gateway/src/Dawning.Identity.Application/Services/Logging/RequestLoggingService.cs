using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Application.Interfaces.Logging;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Logging
{
    /// <summary>
    /// 请求日志服务实现（基于数据库存储）
    /// </summary>
    public class RequestLoggingService : IRequestLoggingService
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<RequestLoggingService> _logger;

        public RequestLoggingService(
            IDbConnection connection,
            ILogger<RequestLoggingService> logger
        )
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task LogRequestAsync(RequestLogEntry entry)
        {
            try
            {
                const string sql =
                    @"
                    INSERT INTO request_logs 
                    (id, request_id, method, path, query_string, status_code, response_time_ms, 
                     client_ip, user_agent, user_id, user_name, request_time, 
                     request_body_size, response_body_size, exception, additional_info)
                    VALUES 
                    (@Id, @RequestId, @Method, @Path, @QueryString, @StatusCode, @ResponseTimeMs,
                     @ClientIp, @UserAgent, @UserId, @UserName, @RequestTime,
                     @RequestBodySize, @ResponseBodySize, @Exception, @AdditionalInfo)";

                await _connection.ExecuteAsync(
                    sql,
                    new
                    {
                        entry.Id,
                        entry.RequestId,
                        entry.Method,
                        entry.Path,
                        entry.QueryString,
                        entry.StatusCode,
                        entry.ResponseTimeMs,
                        entry.ClientIp,
                        entry.UserAgent,
                        entry.UserId,
                        entry.UserName,
                        entry.RequestTime,
                        entry.RequestBodySize,
                        entry.ResponseBodySize,
                        entry.Exception,
                        entry.AdditionalInfo,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log request: {Path}", entry.Path);
            }
        }

        /// <inheritdoc />
        public async Task<PagedRequestLogs> GetLogsAsync(RequestLogQuery query)
        {
            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            if (query.StartTime.HasValue)
            {
                whereConditions.Add("request_time >= @StartTime");
                parameters.Add("StartTime", query.StartTime.Value);
            }

            if (query.EndTime.HasValue)
            {
                whereConditions.Add("request_time <= @EndTime");
                parameters.Add("EndTime", query.EndTime.Value);
            }

            if (!string.IsNullOrEmpty(query.Method))
            {
                whereConditions.Add("method = @Method");
                parameters.Add("Method", query.Method.ToUpper());
            }

            if (!string.IsNullOrEmpty(query.Path))
            {
                whereConditions.Add("path LIKE @Path");
                parameters.Add("Path", $"%{query.Path}%");
            }

            if (query.StatusCode.HasValue)
            {
                whereConditions.Add("status_code = @StatusCode");
                parameters.Add("StatusCode", query.StatusCode.Value);
            }

            if (query.MinStatusCode.HasValue)
            {
                whereConditions.Add("status_code >= @MinStatusCode");
                parameters.Add("MinStatusCode", query.MinStatusCode.Value);
            }

            if (query.MaxStatusCode.HasValue)
            {
                whereConditions.Add("status_code <= @MaxStatusCode");
                parameters.Add("MaxStatusCode", query.MaxStatusCode.Value);
            }

            if (query.UserId.HasValue)
            {
                whereConditions.Add("user_id = @UserId");
                parameters.Add("UserId", query.UserId.Value.ToString());
            }

            if (!string.IsNullOrEmpty(query.ClientIp))
            {
                whereConditions.Add("client_ip = @ClientIp");
                parameters.Add("ClientIp", query.ClientIp);
            }

            if (query.OnlyErrors == true)
            {
                whereConditions.Add("status_code >= 400");
            }

            if (query.SlowRequestThresholdMs.HasValue)
            {
                whereConditions.Add("response_time_ms >= @SlowThreshold");
                parameters.Add("SlowThreshold", query.SlowRequestThresholdMs.Value);
            }

            var whereClause =
                whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            // Get total count
            var countSql = $"SELECT COUNT(*) FROM request_logs {whereClause}";
            var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

            // Get paged data
            var offset = (query.Page - 1) * query.PageSize;
            parameters.Add("Offset", offset);
            parameters.Add("Limit", query.PageSize);

            var dataSql =
                $@"
                SELECT 
                    id as Id, request_id as RequestId, method as Method, path as Path, 
                    query_string as QueryString, status_code as StatusCode, 
                    response_time_ms as ResponseTimeMs, client_ip as ClientIp, 
                    user_agent as UserAgent, user_id as UserId, user_name as UserName,
                    request_time as RequestTime, request_body_size as RequestBodySize,
                    response_body_size as ResponseBodySize, exception as Exception,
                    additional_info as AdditionalInfo
                FROM request_logs 
                {whereClause}
                ORDER BY request_time DESC
                LIMIT @Limit OFFSET @Offset";

            var items = await _connection.QueryAsync<RequestLogEntry>(dataSql, parameters);

            return new PagedRequestLogs
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }

        /// <inheritdoc />
        public async Task<RequestStatistics> GetStatisticsAsync(
            DateTime? startTime = null,
            DateTime? endTime = null
        )
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            var parameters = new { StartTime = start, EndTime = end };

            // Basic statistics
            var basicStatsSql =
                @"
                SELECT 
                    COUNT(*) as TotalRequests,
                    SUM(CASE WHEN status_code >= 200 AND status_code < 300 THEN 1 ELSE 0 END) as SuccessRequests,
                    SUM(CASE WHEN status_code >= 400 AND status_code < 500 THEN 1 ELSE 0 END) as ClientErrors,
                    SUM(CASE WHEN status_code >= 500 THEN 1 ELSE 0 END) as ServerErrors,
                    AVG(response_time_ms) as AverageResponseTimeMs,
                    MAX(response_time_ms) as MaxResponseTimeMs,
                    MIN(response_time_ms) as MinResponseTimeMs
                FROM request_logs
                WHERE request_time >= @StartTime AND request_time <= @EndTime";

            var basicStats = await _connection.QueryFirstAsync<dynamic>(basicStatsSql, parameters);

            var statistics = new RequestStatistics
            {
                TotalRequests = basicStats.TotalRequests ?? 0,
                SuccessRequests = basicStats.SuccessRequests ?? 0,
                ClientErrors = basicStats.ClientErrors ?? 0,
                ServerErrors = basicStats.ServerErrors ?? 0,
                AverageResponseTimeMs = (double)(basicStats.AverageResponseTimeMs ?? 0),
                MaxResponseTimeMs = basicStats.MaxResponseTimeMs ?? 0,
                MinResponseTimeMs = basicStats.MinResponseTimeMs ?? 0,
                StartTime = start,
                EndTime = end,
            };

            // Status code distribution
            var statusDistSql =
                @"
                SELECT status_code as StatusCode, COUNT(*) as Count
                FROM request_logs
                WHERE request_time >= @StartTime AND request_time <= @EndTime
                GROUP BY status_code
                ORDER BY Count DESC";

            var statusDist = await _connection.QueryAsync<dynamic>(statusDistSql, parameters);
            statistics.StatusCodeDistribution = statusDist.ToDictionary(
                x => (int)x.StatusCode,
                x => (long)x.Count
            );

            // Top paths
            var topPathsSql =
                @"
                SELECT 
                    path as Path,
                    COUNT(*) as RequestCount,
                    AVG(response_time_ms) as AverageResponseTimeMs,
                    SUM(CASE WHEN status_code >= 400 THEN 1 ELSE 0 END) as ErrorCount
                FROM request_logs
                WHERE request_time >= @StartTime AND request_time <= @EndTime
                GROUP BY path
                ORDER BY RequestCount DESC
                LIMIT 10";

            var topPaths = await _connection.QueryAsync<PathStatistic>(topPathsSql, parameters);
            statistics.TopPaths = topPaths.ToList();

            // Hourly requests
            var hourlyRequestsSql =
                @"
                SELECT 
                    DATE_FORMAT(request_time, '%Y-%m-%d %H:00') as Hour,
                    COUNT(*) as Count
                FROM request_logs
                WHERE request_time >= @StartTime AND request_time <= @EndTime
                GROUP BY Hour
                ORDER BY Hour";

            var hourlyRequests = await _connection.QueryAsync<dynamic>(
                hourlyRequestsSql,
                parameters
            );
            statistics.HourlyRequests = hourlyRequests.ToDictionary(
                x => (string)x.Hour,
                x => (long)x.Count
            );

            // P95 and P99 (approximate using percentile calculation)
            if (statistics.TotalRequests > 0)
            {
                var percentilesSql =
                    @"
                    SELECT response_time_ms
                    FROM request_logs
                    WHERE request_time >= @StartTime AND request_time <= @EndTime
                    ORDER BY response_time_ms";

                var responseTimes = (
                    await _connection.QueryAsync<long>(percentilesSql, parameters)
                ).ToList();

                if (responseTimes.Count > 0)
                {
                    var p95Index = (int)Math.Ceiling(responseTimes.Count * 0.95) - 1;
                    var p99Index = (int)Math.Ceiling(responseTimes.Count * 0.99) - 1;

                    statistics.P95ResponseTimeMs = responseTimes[
                        Math.Max(0, Math.Min(p95Index, responseTimes.Count - 1))
                    ];
                    statistics.P99ResponseTimeMs = responseTimes[
                        Math.Max(0, Math.Min(p99Index, responseTimes.Count - 1))
                    ];
                }
            }

            return statistics;
        }

        /// <inheritdoc />
        public async Task<int> CleanupOldLogsAsync(int retentionDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            const string sql = "DELETE FROM request_logs WHERE request_time < @CutoffDate";

            var deletedCount = await _connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });

            _logger.LogInformation(
                "Cleaned up {DeletedCount} old request logs older than {RetentionDays} days",
                deletedCount,
                retentionDays
            );

            return deletedCount;
        }
    }
}
