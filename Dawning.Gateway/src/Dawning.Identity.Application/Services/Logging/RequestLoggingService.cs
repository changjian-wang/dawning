using System;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Logging;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models.Monitoring;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Logging
{
    /// <summary>
    /// 请求日志服务实现（基于 Repository 模式）
    /// </summary>
    public class RequestLoggingService : IRequestLoggingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RequestLoggingService> _logger;

        public RequestLoggingService(IUnitOfWork unitOfWork, ILogger<RequestLoggingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task LogRequestAsync(RequestLogEntry entry)
        {
            try
            {
                var log = new RequestLog
                {
                    Id = entry.Id,
                    RequestId = entry.RequestId,
                    Method = entry.Method,
                    Path = entry.Path,
                    QueryString = entry.QueryString,
                    StatusCode = entry.StatusCode,
                    ResponseTimeMs = entry.ResponseTimeMs,
                    ClientIp = entry.ClientIp,
                    UserAgent = entry.UserAgent,
                    UserId = entry.UserId,
                    UserName = entry.UserName,
                    RequestTime = entry.RequestTime,
                    RequestBodySize = entry.RequestBodySize,
                    ResponseBodySize = entry.ResponseBodySize,
                    Exception = entry.Exception,
                    AdditionalInfo = entry.AdditionalInfo,
                };

                await _unitOfWork.RequestLog.InsertAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log request: {Path}", entry.Path);
            }
        }

        /// <inheritdoc />
        public async Task<PagedRequestLogs> GetLogsAsync(RequestLogQuery query)
        {
            var queryModel = new RequestLogQueryModel
            {
                StartTime = query.StartTime,
                EndTime = query.EndTime,
                Method = query.Method,
                Path = query.Path,
                StatusCode = query.StatusCode,
                MinStatusCode = query.MinStatusCode,
                MaxStatusCode = query.MaxStatusCode,
                UserId = query.UserId,
                ClientIp = query.ClientIp,
                OnlyErrors = query.OnlyErrors,
                SlowRequestThresholdMs = query.SlowRequestThresholdMs,
            };

            var result = await _unitOfWork.RequestLog.GetPagedListAsync(
                queryModel,
                query.Page,
                query.PageSize
            );

            return new PagedRequestLogs
            {
                Items = result.Items.Select(MapToEntry).ToList(),
                TotalCount = result.TotalCount,
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

            var stats = await _unitOfWork.RequestLog.GetStatisticsAsync(start, end);

            return new RequestStatistics
            {
                TotalRequests = stats.TotalRequests,
                SuccessRequests = stats.SuccessRequests,
                ClientErrors = stats.ClientErrors,
                ServerErrors = stats.ServerErrors,
                AverageResponseTimeMs = stats.AverageResponseTimeMs,
                MaxResponseTimeMs = stats.MaxResponseTimeMs,
                MinResponseTimeMs = stats.MinResponseTimeMs,
                P95ResponseTimeMs = stats.P95ResponseTimeMs,
                P99ResponseTimeMs = stats.P99ResponseTimeMs,
                StartTime = stats.StartTime,
                EndTime = stats.EndTime,
                StatusCodeDistribution = stats.StatusCodeDistribution,
                TopPaths = stats
                    .TopPaths.Select(p => new PathStatistic
                    {
                        Path = p.Path,
                        RequestCount = p.RequestCount,
                        AverageResponseTimeMs = p.AverageResponseTimeMs,
                        ErrorCount = p.ErrorCount,
                    })
                    .ToList(),
                HourlyRequests = stats.HourlyRequests,
            };
        }

        /// <inheritdoc />
        public async Task<int> CleanupOldLogsAsync(int retentionDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var deletedCount = await _unitOfWork.RequestLog.CleanupOldLogsAsync(cutoffDate);

            _logger.LogInformation(
                "Cleaned up {DeletedCount} old request logs older than {RetentionDays} days",
                deletedCount,
                retentionDays
            );

            return deletedCount;
        }

        private static RequestLogEntry MapToEntry(RequestLog log)
        {
            return new RequestLogEntry
            {
                Id = log.Id,
                RequestId = log.RequestId,
                Method = log.Method,
                Path = log.Path,
                QueryString = log.QueryString,
                StatusCode = log.StatusCode,
                ResponseTimeMs = log.ResponseTimeMs,
                ClientIp = log.ClientIp,
                UserAgent = log.UserAgent,
                UserId = log.UserId,
                UserName = log.UserName,
                RequestTime = log.RequestTime,
                RequestBodySize = log.RequestBodySize,
                ResponseBodySize = log.ResponseBodySize,
                Exception = log.Exception,
                AdditionalInfo = log.AdditionalInfo,
            };
        }
    }
}
