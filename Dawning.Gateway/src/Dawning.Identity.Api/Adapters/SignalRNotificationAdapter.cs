using Dawning.Identity.Api.Hubs;
using Dawning.Identity.Api.Services;
using Dawning.Identity.Application.Interfaces.Notification;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Adapters;

/// <summary>
/// SignalR 实时通知适配器
/// 将 Application 层的 IRealTimeNotificationService 适配到 SignalR Hub
/// </summary>
public class SignalRNotificationAdapter : IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationAdapter> _logger;

    public SignalRNotificationAdapter(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationAdapter> logger
    )
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAlertNotificationAsync(RealTimeAlertNotification alert)
    {
        try
        {
            // 转换为 SignalR 通知模型
            var notification = new AlertNotification
            {
                Id = alert.Id,
                Type = "alert",
                Title = alert.Title,
                Message = alert.Message,
                CreatedAt = alert.CreatedAt,
                Severity = alert.Severity,
                RuleId = alert.RuleId.HasValue ? Guid.Parse(alert.RuleId.Value.ToString()) : null,
                RuleName = alert.RuleName,
                Source = alert.MetricType,
                Value = alert.Value,
                Threshold = alert.Threshold,
                Data = alert.Data,
            };

            // 发送给订阅了 alerts 频道的用户
            await _hubContext.Clients
                .Group("channel_alerts")
                .SendAsync("AlertReceived", notification);

            // 同时发送给管理员
            await _hubContext.Clients
                .Group("role_admin")
                .SendAsync("AlertReceived", notification);

            await _hubContext.Clients
                .Group("role_super_admin")
                .SendAsync("AlertReceived", notification);

            _logger.LogInformation(
                "告警已推送: {Title} ({Severity})",
                alert.Title,
                alert.Severity
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送告警失败: {AlertId}", alert.Id);
        }
    }

    public async Task SendSystemMessageAsync(RealTimeSystemMessage message)
    {
        try
        {
            var notification = new SystemMessage
            {
                Id = message.Id,
                Type = "system",
                Title = message.Title,
                Message = message.Message,
                CreatedAt = message.CreatedAt,
                RequireAcknowledge = message.RequireAcknowledge,
                Priority = message.Priority,
                ExpiresAt = message.ExpiresAt,
                Data = message.Data,
            };

            await _hubContext.Clients.All.SendAsync("SystemMessage", notification);

            _logger.LogInformation("系统消息已推送: {Title}", message.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送系统消息失败");
        }
    }

    public async Task SendToUserAsync(Guid userId, RealTimeNotification notification)
    {
        try
        {
            var msg = new Notification
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                Data = notification.Data,
            };

            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("NotificationReceived", msg);

            _logger.LogDebug(
                "通知已推送给用户 {UserId}: {Title}",
                userId,
                notification.Title
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送通知给用户 {UserId} 失败", userId);
        }
    }

    public async Task SendToRoleAsync(string role, RealTimeNotification notification)
    {
        try
        {
            var msg = new Notification
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                Data = notification.Data,
            };

            await _hubContext.Clients
                .Group($"role_{role}")
                .SendAsync("NotificationReceived", msg);

            _logger.LogDebug(
                "通知已推送给角色 {Role}: {Title}",
                role,
                notification.Title
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送通知给角色 {Role} 失败", role);
        }
    }

    public async Task SendLogEntryAsync(RealTimeLogEntry logEntry)
    {
        try
        {
            // 推送到所有订阅了日志频道的客户端
            await _hubContext.Clients
                .Group("channel_logs_all")
                .SendAsync("LogEntry", logEntry);

            // 根据日志级别推送到特定频道
            var levelChannel = GetLevelChannel(logEntry.Level);
            if (!string.IsNullOrEmpty(levelChannel))
            {
                await _hubContext.Clients
                    .Group($"channel_{levelChannel}")
                    .SendAsync("LogEntry", logEntry);
            }

            _logger.LogDebug(
                "日志已推送: [{Level}] {Message}",
                logEntry.Level,
                logEntry.Message.Length > 50 ? logEntry.Message[..50] + "..." : logEntry.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送日志失败");
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
            _ => null
        };
    }
}
