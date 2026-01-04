using Dawning.Identity.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Notification;

/// <summary>
/// Null implementation of real-time notification service.
/// Used when SignalR is not available.
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
        _logger.LogDebug("Real-time notification service not configured, skipping alert push: {Title}", alert.Title);
        return Task.CompletedTask;
    }

    public Task SendSystemMessageAsync(RealTimeSystemMessage message)
    {
        _logger.LogDebug("Real-time notification service not configured, skipping system message push: {Title}", message.Title);
        return Task.CompletedTask;
    }

    public Task SendLogEntryAsync(RealTimeLogEntry logEntry)
    {
        _logger.LogDebug(
            "Real-time notification service not configured, skipping log entry push: {Level} - {Message}",
            logEntry.Level,
            logEntry.Message
        );
        return Task.CompletedTask;
    }

    public Task SendToUserAsync(Guid userId, RealTimeNotification notification)
    {
        _logger.LogDebug(
            "Real-time notification service not configured, skipping user notification push: UserId={UserId}, Title={Title}",
            userId,
            notification.Title
        );
        return Task.CompletedTask;
    }

    public Task SendToRoleAsync(string role, RealTimeNotification notification)
    {
        _logger.LogDebug(
            "Real-time notification service not configured, skipping role notification push: Role={Role}, Title={Title}",
            role,
            notification.Title
        );
        return Task.CompletedTask;
    }
}
