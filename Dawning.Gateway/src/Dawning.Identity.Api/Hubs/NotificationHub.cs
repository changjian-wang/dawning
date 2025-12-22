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

        // 支持的日志频道（需要管理员权限）
        private static readonly string[] LogChannels =
        [
            "logs_all",
            "logs_error",
            "logs_warning",
            "logs_info",
        ];

        // 支持的通知频道
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
        /// <param name="channel">频道名称 (如: alerts, system, monitoring, logs_all, logs_error)</param>
        public async Task Subscribe(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new HubException("频道名称不能为空");
            }

            var channelLower = channel.ToLower();

            // 检查日志频道权限
            if (LogChannels.Contains(channelLower))
            {
                if (!IsAdmin())
                {
                    throw new HubException("只有管理员才能订阅日志频道");
                }
            }
            else if (!NotificationChannels.Contains(channelLower))
            {
                throw new HubException($"无效的频道名称: {channel}");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel_{channelLower}");
            _logger.LogDebug(
                "用户 {Username} 订阅了频道 {Channel}",
                Context.User?.Identity?.Name,
                channel
            );

            // 通知客户端订阅成功
            await Clients.Caller.SendAsync("Subscribed", channelLower);
        }

        /// <summary>
        /// 取消订阅频道
        /// </summary>
        /// <param name="channel">频道名称</param>
        public async Task Unsubscribe(string channel)
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                var channelLower = channel.ToLower();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel_{channelLower}");
                _logger.LogDebug(
                    "用户 {Username} 取消订阅了频道 {Channel}",
                    Context.User?.Identity?.Name,
                    channel
                );

                // 通知客户端取消订阅成功
                await Clients.Caller.SendAsync("Unsubscribed", channelLower);
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

        /// <summary>
        /// 获取当前连接的订阅状态
        /// </summary>
        public async Task GetSubscriptions()
        {
            // 这个方法让客户端可以查询当前连接订阅了哪些频道
            // 服务端暂时无法直接获取，客户端需自己维护状态
            await Clients.Caller.SendAsync(
                "SubscriptionStatus",
                new { ConnectionId = Context.ConnectionId, Message = "请在客户端维护订阅状态" }
            );
        }

        /// <summary>
        /// 检查当前用户是否是管理员
        /// </summary>
        private bool IsAdmin()
        {
            var roles = Context.User?.FindAll("role")?.Select(c => c.Value.ToLower()) ?? [];
            return roles.Any(r => r is "admin" or "super_admin" or "auditor");
        }
    }
}
