using Dawning.Identity.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Notification;

/// <summary>
/// 空实现的实时通知服务
/// 当没有 SignalR 可用时使用此实现
/// </summary>
public class NullRealTimeNotificationService : IRealTimeNotificationService
{
    private readonly ILogger<NullRealTimeNotificationService> _logger;

    public NullRealTimeNotificationService(ILogger<NullRealTimeNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendAlertNotificationAsync(RealTimeAlertNotification alert)
    {
        _logger.LogDebug(
            "实时通知服务未配置，跳过告警推送: {Title}",
            alert.Title
        );
        return Task.CompletedTask;
    }

    public Task SendSystemMessageAsync(RealTimeSystemMessage message)
    {
        _logger.LogDebug(
            "实时通知服务未配置，跳过系统消息推送: {Title}",
            message.Title
        );
        return Task.CompletedTask;
    }

    public Task SendToUserAsync(Guid userId, RealTimeNotification notification)
    {
        _logger.LogDebug(
            "实时通知服务未配置，跳过用户通知推送: UserId={UserId}, Title={Title}",
            userId,
            notification.Title
        );
        return Task.CompletedTask;
    }

    public Task SendToRoleAsync(string role, RealTimeNotification notification)
    {
        _logger.LogDebug(
            "实时通知服务未配置，跳过角色通知推送: Role={Role}, Title={Title}",
            role,
            notification.Title
        );
        return Task.CompletedTask;
    }
}
