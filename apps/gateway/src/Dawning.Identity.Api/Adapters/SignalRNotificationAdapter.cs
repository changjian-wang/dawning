using Dawning.Identity.Api.Hubs;
using Dawning.Identity.Api.Services;
using Dawning.Identity.Application.Interfaces.Notification;
using Microsoft.AspNetCore.SignalR;

namespace Dawning.Identity.Api.Adapters;

/// <summary>
/// SignalR real-time notification adapter
/// Adapts the Application layer's IRealTimeNotificationService to SignalR Hub
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
            // Convert to SignalR notification model
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

            // Send to users subscribed to alerts channel
            await _hubContext
                .Clients.Group("channel_alerts")
                .SendAsync("AlertReceived", notification);

            // Also send to administrators
            await _hubContext.Clients.Group("role_admin").SendAsync("AlertReceived", notification);

            await _hubContext
                .Clients.Group("role_super_admin")
                .SendAsync("AlertReceived", notification);

            _logger.LogInformation("Alert pushed: {Title} ({Severity})", alert.Title, alert.Severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push alert: {AlertId}", alert.Id);
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

            _logger.LogInformation("System message pushed: {Title}", message.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push system message");
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

            await _hubContext
                .Clients.Group($"user_{userId}")
                .SendAsync("NotificationReceived", msg);

            _logger.LogDebug("Notification pushed to user {UserId}: {Title}", userId, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push notification to user {UserId}", userId);
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

            await _hubContext.Clients.Group($"role_{role}").SendAsync("NotificationReceived", msg);

            _logger.LogDebug("Notification pushed to role {Role}: {Title}", role, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push notification to role {Role}", role);
        }
    }

    public async Task SendLogEntryAsync(RealTimeLogEntry logEntry)
    {
        try
        {
            // Push to all clients subscribed to log channel
            await _hubContext.Clients.Group("channel_logs_all").SendAsync("LogEntry", logEntry);

            // Push to specific channel based on log level
            var levelChannel = GetLevelChannel(logEntry.Level);
            if (!string.IsNullOrEmpty(levelChannel))
            {
                await _hubContext
                    .Clients.Group($"channel_{levelChannel}")
                    .SendAsync("LogEntry", logEntry);
            }

            _logger.LogDebug(
                "Log pushed: [{Level}] {Message}",
                logEntry.Level,
                logEntry.Message.Length > 50 ? logEntry.Message[..50] + "..." : logEntry.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push log");
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
            _ => null,
        };
    }
}
