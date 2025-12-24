using Dawning.Identity.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Services
{
    /// <summary>
    /// 实时通知服务接口
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 发送告警通知给管理员
        /// </summary>
        Task SendAlertAsync(AlertNotification alert);

        /// <summary>
        /// 发送系统消息给所有用户
        /// </summary>
        Task SendSystemMessageAsync(SystemMessage message);

        /// <summary>
        /// 发送通知给特定用户
        /// </summary>
        Task SendToUserAsync(Guid userId, Notification notification);

        /// <summary>
        /// 发送通知给特定角色的用户
        /// </summary>
        Task SendToRoleAsync(string role, Notification notification);

        /// <summary>
        /// 发送通知给订阅了特定频道的用户
        /// </summary>
        Task SendToChannelAsync(string channel, Notification notification);
    }

    /// <summary>
    /// 实时通知服务实现
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
                await _hubContext.Clients
                    .Group("channel_alerts")
                    .SendAsync("AlertReceived", alert);

                // 同时发送给管理员
                await _hubContext.Clients
                    .Group("role_admin")
                    .SendAsync("AlertReceived", alert);

                await _hubContext.Clients
                    .Group("role_super_admin")
                    .SendAsync("AlertReceived", alert);

                _logger.LogInformation(
                    "告警通知已发送: {AlertType} - {Message}",
                    alert.Type,
                    alert.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送告警通知失败: {AlertId}", alert.Id);
            }
        }

        /// <inheritdoc />
        public async Task SendSystemMessageAsync(SystemMessage message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("SystemMessage", message);
                _logger.LogInformation("系统消息已发送: {Title}", message.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送系统消息失败");
            }
        }

        /// <inheritdoc />
        public async Task SendToUserAsync(Guid userId, Notification notification)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("NotificationReceived", notification);

                _logger.LogDebug(
                    "通知已发送给用户 {UserId}: {Type}",
                    userId,
                    notification.Type
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知给用户 {UserId} 失败", userId);
            }
        }

        /// <inheritdoc />
        public async Task SendToRoleAsync(string role, Notification notification)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"role_{role}")
                    .SendAsync("NotificationReceived", notification);

                _logger.LogDebug(
                    "通知已发送给角色 {Role}: {Type}",
                    role,
                    notification.Type
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知给角色 {Role} 失败", role);
            }
        }

        /// <inheritdoc />
        public async Task SendToChannelAsync(string channel, Notification notification)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"channel_{channel}")
                    .SendAsync("NotificationReceived", notification);

                _logger.LogDebug(
                    "通知已发送到频道 {Channel}: {Type}",
                    channel,
                    notification.Type
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知到频道 {Channel} 失败", channel);
            }
        }
    }

    #region Notification Models

    /// <summary>
    /// 基础通知
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 通知类型
        /// </summary>
        public string Type { get; set; } = "info";

        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 附加数据
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// 告警通知
    /// </summary>
    public class AlertNotification : Notification
    {
        /// <summary>
        /// 告警级别 (info, warning, error, critical)
        /// </summary>
        public string Severity { get; set; } = "warning";

        /// <summary>
        /// 告警规则ID
        /// </summary>
        public Guid? RuleId { get; set; }

        /// <summary>
        /// 告警规则名称
        /// </summary>
        public string? RuleName { get; set; }

        /// <summary>
        /// 告警来源
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// 告警值
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        public decimal? Threshold { get; set; }
    }

    /// <summary>
    /// 系统消息
    /// </summary>
    public class SystemMessage : Notification
    {
        /// <summary>
        /// 是否需要用户确认
        /// </summary>
        public bool RequireAcknowledge { get; set; }

        /// <summary>
        /// 消息优先级 (low, normal, high, urgent)
        /// </summary>
        public string Priority { get; set; } = "normal";

        /// <summary>
        /// 过期时间（null表示永不过期）
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    #endregion
}
