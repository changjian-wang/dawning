using Dawning.Identity.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Services
{
    /// <summary>
    /// 日志流服务 - 用于实时推送日志到订阅的客户端
    /// </summary>
    public interface ILogStreamService
    {
        /// <summary>
        /// 推送日志条目到订阅的客户端
        /// </summary>
        Task PushLogAsync(LogEntry entry);

        /// <summary>
        /// 推送批量日志条目
        /// </summary>
        Task PushLogsAsync(IEnumerable<LogEntry> entries);
    }

    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 日志级别 (Information, Warning, Error, Debug)
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息（如果有）
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 日志来源/类别
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 请求路径（如果有）
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// 用户ID（如果有）
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 用户名（如果有）
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 额外属性
        /// </summary>
        public Dictionary<string, object>? Properties { get; set; }
    }

    /// <summary>
    /// 日志流服务实现
    /// </summary>
    public class LogStreamService : ILogStreamService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<LogStreamService> _logger;

        public LogStreamService(
            IHubContext<NotificationHub> hubContext,
            ILogger<LogStreamService> logger
        )
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task PushLogAsync(LogEntry entry)
        {
            try
            {
                // 推送到对应级别的频道
                var levelChannel = GetLevelChannel(entry.Level);

                // 推送到所有日志频道
                await _hubContext.Clients.Group("channel_logs_all").SendAsync("LogEntry", entry);

                // 推送到特定级别频道
                if (!string.IsNullOrEmpty(levelChannel))
                {
                    await _hubContext
                        .Clients.Group($"channel_{levelChannel}")
                        .SendAsync("LogEntry", entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送日志条目失败: {Message}", entry.Message);
            }
        }

        /// <inheritdoc/>
        public async Task PushLogsAsync(IEnumerable<LogEntry> entries)
        {
            foreach (var entry in entries)
            {
                await PushLogAsync(entry);
            }
        }

        /// <summary>
        /// 获取日志级别对应的频道名
        /// </summary>
        private static string? GetLevelChannel(string level)
        {
            return level.ToLower() switch
            {
                "error" or "critical" or "fatal" => "logs_error",
                "warning" or "warn" => "logs_warning",
                "information" or "info" => "logs_info",
                "debug" or "trace" => null, // Debug/Trace 只推送到 logs_all
                _ => null,
            };
        }
    }
}
