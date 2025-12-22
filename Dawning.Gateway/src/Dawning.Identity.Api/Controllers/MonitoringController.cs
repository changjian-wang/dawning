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
    /// 监控和日志API控制器
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
        /// 获取请求日志列表
        /// </summary>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="method">HTTP方法</param>
        /// <param name="path">路径过滤</param>
        /// <param name="statusCode">状态码</param>
        /// <param name="minStatusCode">最小状态码</param>
        /// <param name="maxStatusCode">最大状态码</param>
        /// <param name="userId">用户ID</param>
        /// <param name="clientIp">客户端IP</param>
        /// <param name="onlyErrors">只显示错误</param>
        /// <param name="slowRequestThresholdMs">慢请求阈值</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>分页请求日志</returns>
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
        /// 获取请求统计信息
        /// </summary>
        /// <param name="startTime">起始时间（默认24小时前）</param>
        /// <param name="endTime">结束时间（默认现在）</param>
        /// <returns>请求统计信息</returns>
        [HttpGet("statistics")]
        [CacheResponse(60, VaryByQueryKeys = "startTime,endTime")] // 缓存1分钟
        public async Task<ActionResult<RequestStatistics>> GetStatistics(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null
        )
        {
            var statistics = await _requestLoggingService.GetStatisticsAsync(startTime, endTime);
            return Ok(statistics);
        }

        /// <summary>
        /// 获取系统性能指标
        /// </summary>
        /// <returns>系统性能指标</returns>
        [HttpGet("performance")]
        [CacheResponse(30)] // 缓存30秒
        public ActionResult<SystemPerformanceMetrics> GetPerformanceMetrics()
        {
            var process = Process.GetCurrentProcess();
            var metrics = new SystemPerformanceMetrics
            {
                // 进程信息
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                StartTime = process.StartTime,
                Uptime = DateTime.Now - process.StartTime,

                // 内存信息
                WorkingSetMemoryMb = process.WorkingSet64 / (1024.0 * 1024.0),
                PrivateMemoryMb = process.PrivateMemorySize64 / (1024.0 * 1024.0),
                VirtualMemoryMb = process.VirtualMemorySize64 / (1024.0 * 1024.0),
                ManagedMemoryMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0),

                // GC 信息
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                TotalAllocatedMb = GC.GetTotalAllocatedBytes(false) / (1024.0 * 1024.0),

                // 线程信息
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,

                // CPU 信息
                TotalProcessorTime = process.TotalProcessorTime,
                UserProcessorTime = process.UserProcessorTime,
                PrivilegedProcessorTime = process.PrivilegedProcessorTime,

                // 时间戳
                Timestamp = DateTime.UtcNow,
            };

            return Ok(metrics);
        }

        /// <summary>
        /// 获取实时监控数据（用于仪表盘轮询）
        /// </summary>
        /// <returns>实时监控数据</returns>
        [HttpGet("realtime")]
        [CacheResponse(15)] // 缓存15秒
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
        /// 清理过期日志
        /// </summary>
        /// <param name="retentionDays">保留天数</param>
        /// <returns>删除的日志数量</returns>
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
        /// 获取用户统计信息
        /// </summary>
        /// <returns>用户统计数据</returns>
        [HttpGet("user-statistics")]
        [CacheResponse(120)] // 缓存2分钟
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
        {
            var stats = await _userService.GetUserStatisticsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// 获取最近活跃用户
        /// </summary>
        /// <param name="count">返回数量（默认10）</param>
        /// <returns>最近活跃用户列表</returns>
        [HttpGet("recent-active-users")]
        [CacheResponse(60, VaryByQueryKeys = "count")] // 缓存1分钟
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
        /// 触发垃圾回收（仅用于调试）
        /// </summary>
        /// <returns>GC结果</returns>
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
    /// 系统性能指标
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
    /// 实时监控数据
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
    /// 日志清理结果
    /// </summary>
    public class LogCleanupResult
    {
        public int DeletedCount { get; set; }
        public int RetentionDays { get; set; }
        public DateTime CleanupTime { get; set; }
    }

    /// <summary>
    /// GC结果
    /// </summary>
    public class GcResult
    {
        public double MemoryBeforeMb { get; set; }
        public double MemoryAfterMb { get; set; }
        public double FreedMb { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
