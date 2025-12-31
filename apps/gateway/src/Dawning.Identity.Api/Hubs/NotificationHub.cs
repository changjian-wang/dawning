using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Hubs
{
    /// <summary>
    /// Real-time notification Hub
    /// Provides real-time push alerts, system messages, user status updates, etc.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        // Supported log channels (requires admin permission)
        private static readonly string[] LogChannels =
        [
            "logs_all",
            "logs_error",
            "logs_warning",
            "logs_info",
        ];

        // Supported notification channels
        private static readonly string[] NotificationChannels =
        [
            "alerts",
            "system",
            "monitoring",
            "audit",
        ];

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when client connects
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            var username = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                // Add to corresponding groups based on roles
                var roles = Context.User?.FindAll("role")?.Select(c => c.Value) ?? [];
                foreach (var role in roles)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
                }

                _logger.LogInformation(
                    "User {Username} (ID: {UserId}) connected, ConnectionId: {ConnectionId}",
                    username,
                    userId,
                    Context.ConnectionId
                );
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when client disconnects
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            var username = Context.User?.Identity?.Name;

            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "User {Username} connection disconnected abnormally, ConnectionId: {ConnectionId}",
                    username,
                    Context.ConnectionId
                );
            }
            else
            {
                _logger.LogInformation(
                    "User {Username} disconnected, ConnectionId: {ConnectionId}",
                    username,
                    Context.ConnectionId
                );
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to a specific channel
        /// </summary>
        /// <param name="channel">Channel name (e.g.: alerts, system, monitoring, logs_all, logs_error)</param>
        public async Task Subscribe(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new HubException("Channel name cannot be empty");
            }

            var channelLower = channel.ToLower();

            // Check log channel permissions
            if (LogChannels.Contains(channelLower))
            {
                if (!IsAdmin())
                {
                    throw new HubException("Only administrators can subscribe to log channels");
                }
            }
            else if (!NotificationChannels.Contains(channelLower))
            {
                throw new HubException($"Invalid channel name: {channel}");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel_{channelLower}");
            _logger.LogDebug(
                "User {Username} subscribed to channel {Channel}",
                Context.User?.Identity?.Name,
                channel
            );

            // Notify client subscription successful
            await Clients.Caller.SendAsync("Subscribed", channelLower);
        }

        /// <summary>
        /// Unsubscribe from channel
        /// </summary>
        /// <param name="channel">Channel name</param>
        public async Task Unsubscribe(string channel)
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                var channelLower = channel.ToLower();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel_{channelLower}");
                _logger.LogDebug(
                    "User {Username} unsubscribed from channel {Channel}",
                    Context.User?.Identity?.Name,
                    channel
                );

                // Notify client unsubscription successful
                await Clients.Caller.SendAsync("Unsubscribed", channelLower);
            }
        }

        /// <summary>
        /// Acknowledge notification receipt
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        public async Task AcknowledgeNotification(string notificationId)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation(
                "User {UserId} acknowledged notification {NotificationId}",
                userId,
                notificationId
            );

            // Can record acknowledgment status to database here
            await Clients.Caller.SendAsync("NotificationAcknowledged", notificationId);
        }

        /// <summary>
        /// Get current connection subscription status
        /// </summary>
        public async Task GetSubscriptions()
        {
            // This method lets clients query which channels the current connection is subscribed to
            // Server cannot directly get this information, client needs to maintain state
            await Clients.Caller.SendAsync(
                "SubscriptionStatus",
                new { ConnectionId = Context.ConnectionId, Message = "Please maintain subscription status on client side" }
            );
        }

        /// <summary>
        /// Check if current user is an administrator
        /// </summary>
        private bool IsAdmin()
        {
            var roles = Context.User?.FindAll("role")?.Select(c => c.Value.ToLower()) ?? [];
            return roles.Any(r => r is "admin" or "super_admin" or "auditor");
        }
    }
}
