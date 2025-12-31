using Dawning.Identity.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Services
{
    /// <summary>
    /// Real-time notification service interface
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send alert notification to administrators
        /// </summary>
        Task SendAlertAsync(AlertNotification alert);

        /// <summary>
        /// Send system message to all users
        /// </summary>
        Task SendSystemMessageAsync(SystemMessage message);

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        Task SendToUserAsync(Guid userId, Notification notification);

        /// <summary>
        /// Send notification to users with specific role
        /// </summary>
        Task SendToRoleAsync(string role, Notification notification);

        /// <summary>
        /// Send notification to users subscribed to specific channel
        /// </summary>
        Task SendToChannelAsync(string channel, Notification notification);
    }

    /// <summary>
    /// Real-time notification service implementation
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger
        )
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SendAlertAsync(AlertNotification alert)
        {
            try
            {
                // 发送给订阅了 alerts 频道的用户
                await _hubContext.Clients.Group("channel_alerts").SendAsync("AlertReceived", alert);

                // 同时发送给管理员
                await _hubContext.Clients.Group("role_admin").SendAsync("AlertReceived", alert);

                await _hubContext
                    .Clients.Group("role_super_admin")
                    .SendAsync("AlertReceived", alert);

                _logger.LogInformation(
                    "Alert notification sent: {AlertType} - {Message}",
                    alert.Type,
                    alert.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert notification: {AlertId}", alert.Id);
            }
        }

        /// <inheritdoc />
        public async Task SendSystemMessageAsync(SystemMessage message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("SystemMessage", message);
                _logger.LogInformation("System message sent: {Title}", message.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send system message");
            }
        }

        /// <inheritdoc />
        public async Task SendToUserAsync(Guid userId, Notification notification)
        {
            try
            {
                await _hubContext
                    .Clients.Group($"user_{userId}")
                    .SendAsync("NotificationReceived", notification);

                _logger.LogDebug("Notification sent to user {UserId}: {Type}", userId, notification.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            }
        }

        /// <inheritdoc />
        public async Task SendToRoleAsync(string role, Notification notification)
        {
            try
            {
                await _hubContext
                    .Clients.Group($"role_{role}")
                    .SendAsync("NotificationReceived", notification);

                _logger.LogDebug("Notification sent to role {Role}: {Type}", role, notification.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to role {Role}", role);
            }
        }

        /// <inheritdoc />
        public async Task SendToChannelAsync(string channel, Notification notification)
        {
            try
            {
                await _hubContext
                    .Clients.Group($"channel_{channel}")
                    .SendAsync("NotificationReceived", notification);

                _logger.LogDebug("Notification sent to channel {Channel}: {Type}", channel, notification.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to channel {Channel}", channel);
            }
        }
    }

    #region Notification Models

    /// <summary>
    /// Base notification
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Notification ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Notification type
        /// </summary>
        public string Type { get; set; } = "info";

        /// <summary>
        /// Notification title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notification content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// Alert notification
    /// </summary>
    public class AlertNotification : Notification
    {
        /// <summary>
        /// Alert severity (info, warning, error, critical)
        /// </summary>
        public string Severity { get; set; } = "warning";

        /// <summary>
        /// Alert rule ID
        /// </summary>
        public Guid? RuleId { get; set; }

        /// <summary>
        /// Alert rule name
        /// </summary>
        public string? RuleName { get; set; }

        /// <summary>
        /// Alert source
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Alert value
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Threshold
        /// </summary>
        public decimal? Threshold { get; set; }
    }

    /// <summary>
    /// System message
    /// </summary>
    public class SystemMessage : Notification
    {
        /// <summary>
        /// Whether user acknowledgment is required
        /// </summary>
        public bool RequireAcknowledge { get; set; }

        /// <summary>
        /// Message priority (low, normal, high, urgent)
        /// </summary>
        public string Priority { get; set; } = "normal";

        /// <summary>
        /// Expiration time (null means never expires)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    #endregion
}
