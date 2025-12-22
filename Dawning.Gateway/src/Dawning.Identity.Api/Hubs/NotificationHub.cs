using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Hubs
{
    /// <summary>
    /// 实时通知 Hub
    /// 提供实时推送告警、系统消息、用户状态更新等功能
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 客户端连接时调用
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            var username = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                // 将用户加入其个人组
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                // 根据角色加入对应组
                var roles = Context.User?.FindAll("role")?.Select(c => c.Value) ?? [];
                foreach (var role in roles)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
                }

                _logger.LogInformation(
                    "用户 {Username} (ID: {UserId}) 已连接，ConnectionId: {ConnectionId}",
                    username,
                    userId,
                    Context.ConnectionId
                );
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 客户端断开连接时调用
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            var username = Context.User?.Identity?.Name;

            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "用户 {Username} 连接异常断开，ConnectionId: {ConnectionId}",
                    username,
                    Context.ConnectionId
                );
            }
            else
            {
                _logger.LogInformation(
                    "用户 {Username} 已断开连接，ConnectionId: {ConnectionId}",
                    username,
                    Context.ConnectionId
                );
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 订阅特定频道
        /// </summary>
        /// <param name="channel">频道名称 (如: alerts, system, monitoring)</param>
        public async Task Subscribe(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new HubException("频道名称不能为空");
            }

            var allowedChannels = new[] { "alerts", "system", "monitoring", "audit" };
            if (!allowedChannels.Contains(channel.ToLower()))
            {
                throw new HubException($"无效的频道名称: {channel}");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel_{channel}");
            _logger.LogDebug(
                "用户 {Username} 订阅了频道 {Channel}",
                Context.User?.Identity?.Name,
                channel
            );
        }

        /// <summary>
        /// 取消订阅频道
        /// </summary>
        /// <param name="channel">频道名称</param>
        public async Task Unsubscribe(string channel)
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel_{channel}");
                _logger.LogDebug(
                    "用户 {Username} 取消订阅了频道 {Channel}",
                    Context.User?.Identity?.Name,
                    channel
                );
            }
        }

        /// <summary>
        /// 确认收到通知
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        public async Task AcknowledgeNotification(string notificationId)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation(
                "用户 {UserId} 确认收到通知 {NotificationId}",
                userId,
                notificationId
            );

            // 可以在这里记录确认状态到数据库
            await Clients.Caller.SendAsync("NotificationAcknowledged", notificationId);
        }
    }
}
