using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dawning.Identity.Application.Attributes;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers
{
    /// <summary>
    /// Monitoring and Logging API Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "admin")]
    public class MonitoringController : ControllerBase
    {
        private readonly IRequestLoggingService _requestLoggingService;
        private readonly IUserService _userService;

        public MonitoringController(
            IRequestLoggingService requestLoggingService,
            IUserService userService
        )
        {
            _requestLoggingService = requestLoggingService;
            _userService = userService;
        }

        /// <summary>
        /// Get request log list
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="method">HTTP method</param>
        /// <param name="path">Path filter</param>
        /// <param name="statusCode">Status code</param>
        /// <param name="minStatusCode">Minimum status code</param>
        /// <param name="maxStatusCode">Maximum status code</param>
        /// <param name="userId">User ID</param>
        /// <param name="clientIp">Client IP</param>
        /// <param name="onlyErrors">Show only errors</param>
        /// <param name="slowRequestThresholdMs">Slow request threshold</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged request logs</returns>
        [HttpGet("logs")]
        public async Task<ActionResult<PagedRequestLogs>> GetLogs(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null,
            [FromQuery] string? method = null,
            [FromQuery] string? path = null,
            [FromQuery] int? statusCode = null,
            [FromQuery] int? minStatusCode = null,
            [FromQuery] int? maxStatusCode = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] string? clientIp = null,
            [FromQuery] bool? onlyErrors = null,
            [FromQuery] long? slowRequestThresholdMs = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var query = new RequestLogQuery
            {
                StartTime = startTime,
                EndTime = endTime,
                Method = method,
                Path = path,
                StatusCode = statusCode,
                MinStatusCode = minStatusCode,
                MaxStatusCode = maxStatusCode,
                UserId = userId,
                ClientIp = clientIp,
                OnlyErrors = onlyErrors,
                SlowRequestThresholdMs = slowRequestThresholdMs,
                Page = page,
                PageSize = Math.Min(pageSize, 100), // Limit page size
            };

            var logs = await _requestLoggingService.GetLogsAsync(query);
            return Ok(logs);
        }

        /// <summary>
        /// Get request statistics
        /// </summary>
        /// <param name="startTime">Start time (default 24 hours ago)</param>
        /// <param name="endTime">End time (default now)</param>
        /// <returns>Request statistics</returns>
        [HttpGet("statistics")]
        [CacheResponse(60, VaryByQueryKeys = "startTime,endTime")] // Cache for 1 minute
        public async Task<ActionResult<RequestStatistics>> GetStatistics(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null
        )
        {
            var statistics = await _requestLoggingService.GetStatisticsAsync(startTime, endTime);
            return Ok(statistics);
        }

        /// <summary>
        /// Get system performance metrics
        /// </summary>
        /// <returns>System performance metrics</returns>
        [HttpGet("performance")]
        [CacheResponse(30)] // Cache for 30 seconds
        public ActionResult<SystemPerformanceMetrics> GetPerformanceMetrics()
        {
            var process = Process.GetCurrentProcess();
            var metrics = new SystemPerformanceMetrics
            {
                // Process information
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                StartTime = process.StartTime,
                Uptime = DateTime.Now - process.StartTime,

                // Memory information
                WorkingSetMemoryMb = process.WorkingSet64 / (1024.0 * 1024.0),
                PrivateMemoryMb = process.PrivateMemorySize64 / (1024.0 * 1024.0),
                VirtualMemoryMb = process.VirtualMemorySize64 / (1024.0 * 1024.0),
                ManagedMemoryMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0),

                // GC information
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                TotalAllocatedMb = GC.GetTotalAllocatedBytes(false) / (1024.0 * 1024.0),

                // Thread information
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,

                // CPU information
                TotalProcessorTime = process.TotalProcessorTime,
                UserProcessorTime = process.UserProcessorTime,
                PrivilegedProcessorTime = process.PrivilegedProcessorTime,

                // Timestamp
                Timestamp = DateTime.UtcNow,
            };

            return Ok(metrics);
        }

        /// <summary>
        /// Get real-time monitoring data (for dashboard polling)
        /// </summary>
        /// <returns>Real-time monitoring data</returns>
        [HttpGet("realtime")]
        [CacheResponse(15)] // Cache for 15 seconds
        public async Task<ActionResult<RealtimeMonitoringData>> GetRealtimeData()
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);
            var oneHourAgo = now.AddHours(-1);

            // 获取最近1分钟的统计
            var recentStats = await _requestLoggingService.GetStatisticsAsync(oneMinuteAgo, now);

            // 获取最近1小时的统计
            var hourlyStats = await _requestLoggingService.GetStatisticsAsync(oneHourAgo, now);

            var process = Process.GetCurrentProcess();

            var data = new RealtimeMonitoringData
            {
                // 请求指标
                RequestsPerMinute = recentStats.TotalRequests,
                RequestsPerHour = hourlyStats.TotalRequests,
                ErrorsPerMinute = recentStats.ClientErrors + recentStats.ServerErrors,
                ErrorsPerHour = hourlyStats.ClientErrors + hourlyStats.ServerErrors,
                AverageResponseTimeMs = recentStats.AverageResponseTimeMs,

                // 系统指标
                MemoryUsageMb = process.WorkingSet64 / (1024.0 * 1024.0),
                ManagedMemoryMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0),
                ThreadCount = process.Threads.Count,
                Uptime = DateTime.Now - process.StartTime,

                // 时间戳
                Timestamp = now,
            };

            return Ok(data);
        }

        /// <summary>
        /// Clean up expired logs
        /// </summary>
        /// <param name="retentionDays">Retention days</param>
        /// <returns>Number of deleted logs</returns>
        [HttpPost("logs/cleanup")]
        public async Task<ActionResult<LogCleanupResult>> CleanupLogs(
            [FromQuery] int retentionDays = 30
        )
        {
            if (retentionDays < 1)
            {
                return BadRequest("Retention days must be at least 1");
            }

            var deletedCount = await _requestLoggingService.CleanupOldLogsAsync(retentionDays);

            return Ok(
                new LogCleanupResult
                {
                    DeletedCount = deletedCount,
                    RetentionDays = retentionDays,
                    CleanupTime = DateTime.UtcNow,
                }
            );
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        /// <returns>User statistics data</returns>
        [HttpGet("user-statistics")]
        [CacheResponse(120)] // Cache for 2 minutes
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
        {
            var stats = await _userService.GetUserStatisticsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get recently active users
        /// </summary>
        /// <param name="count">Return count (default 10)</param>
        /// <returns>Recently active user list</returns>
        [HttpGet("recent-active-users")]
        [CacheResponse(60, VaryByQueryKeys = "count")] // Cache for 1 minute
        public async Task<ActionResult<IEnumerable<RecentActiveUserDto>>> GetRecentActiveUsers(
            [FromQuery] int count = 10
        )
        {
            if (count < 1)
                count = 10;
            if (count > 100)
                count = 100;

            var users = await _userService.GetRecentActiveUsersAsync(count);
            return Ok(users);
        }

        /// <summary>
        /// Trigger garbage collection (for debugging only)
        /// </summary>
        /// <returns>GC result</returns>
        [HttpPost("gc")]
        public ActionResult<GcResult> TriggerGarbageCollection()
        {
            var before = GC.GetTotalMemory(false);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var after = GC.GetTotalMemory(true);

            return Ok(
                new GcResult
                {
                    MemoryBeforeMb = before / (1024.0 * 1024.0),
                    MemoryAfterMb = after / (1024.0 * 1024.0),
                    FreedMb = (before - after) / (1024.0 * 1024.0),
                    Timestamp = DateTime.UtcNow,
                }
            );
        }
    }

    /// <summary>
    /// System Performance Metrics
    /// </summary>
    public class SystemPerformanceMetrics
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime { get; set; }

        public double WorkingSetMemoryMb { get; set; }
        public double PrivateMemoryMb { get; set; }
        public double VirtualMemoryMb { get; set; }
        public double ManagedMemoryMb { get; set; }

        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public double TotalAllocatedMb { get; set; }

        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }

        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Real-time Monitoring Data
    /// </summary>
    public class RealtimeMonitoringData
    {
        public long RequestsPerMinute { get; set; }
        public long RequestsPerHour { get; set; }
        public long ErrorsPerMinute { get; set; }
        public long ErrorsPerHour { get; set; }
        public double AverageResponseTimeMs { get; set; }

        public double MemoryUsageMb { get; set; }
        public double ManagedMemoryMb { get; set; }
        public int ThreadCount { get; set; }
        public TimeSpan Uptime { get; set; }

        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Log Cleanup Result
    /// </summary>
    public class LogCleanupResult
    {
        public int DeletedCount { get; set; }
        public int RetentionDays { get; set; }
        public DateTime CleanupTime { get; set; }
    }

    /// <summary>
    /// GC Result
    /// </summary>
    public class GcResult
    {
        public double MemoryBeforeMb { get; set; }
        public double MemoryAfterMb { get; set; }
        public double FreedMb { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
