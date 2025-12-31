using Dawning.Identity.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Services
{
    /// <summary>
    /// Log stream service - for real-time log push to subscribed clients
    /// </summary>
    public interface ILogStreamService
    {
        /// <summary>
        /// Push log entry to subscribed clients
        /// </summary>
        Task PushLogAsync(LogEntry entry);

        /// <summary>
        /// Push batch log entries
        /// </summary>
        Task PushLogsAsync(IEnumerable<LogEntry> entries);
    }

    /// <summary>
    /// Log entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Log ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Log level (Information, Warning, Error, Debug)
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// Log message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception information (if any)
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Log source/category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Request path (if any)
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// User ID (if any)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Username (if any)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Extra properties
        /// </summary>
        public Dictionary<string, object>? Properties { get; set; }
    }

    /// <summary>
    /// Log stream service implementation
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
                // Push to corresponding level channel
                var levelChannel = GetLevelChannel(entry.Level);

                // Push to all logs channel
                await _hubContext.Clients.Group("channel_logs_all").SendAsync("LogEntry", entry);

                // Push to specific level channel
                if (!string.IsNullOrEmpty(levelChannel))
                {
                    await _hubContext
                        .Clients.Group($"channel_{levelChannel}")
                        .SendAsync("LogEntry", entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push log entry: {Message}", entry.Message);
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
        /// Get channel name corresponding to log level
        /// </summary>
        private static string? GetLevelChannel(string level)
        {
            return level.ToLower() switch
            {
                "error" or "critical" or "fatal" => "logs_error",
                "warning" or "warn" => "logs_warning",
                "information" or "info" => "logs_info",
                "debug" or "trace" => null, // Debug/Trace only push to logs_all
                _ => null,
            };
        }
    }
}
