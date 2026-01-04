using Dawning.Identity.Application.Dtos.IntegrationEvents;
using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.EventHandlers;

/// <summary>
/// User created event handler
/// In-process handling: record audit log, send welcome email, etc.
/// </summary>
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<UserCreatedEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserCreatedEvent - UserId: {UserId}, UserName: {UserName}",
            notification.UserId,
            notification.UserName
        );

        // 1. Publish audit log to Kafka
        await _integrationEventBus.PublishAsync(
            new AuditLogIntegrationEvent
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                Action = "UserCreated",
                EntityType = "User",
                EntityId = notification.UserId.ToString(),
                Description = $"User '{notification.UserName}' was created",
            },
            cancellationToken: cancellationToken
        );

        // 2. If email exists, send welcome email
        if (!string.IsNullOrEmpty(notification.Email))
        {
            await _integrationEventBus.PublishAsync(
                new EmailIntegrationEvent
                {
                    To = notification.Email,
                    Subject = "Welcome to Dawning Gateway",
                    Body =
                        $"<h1>Welcome, {notification.UserName}!</h1><p>Your account has been created successfully.</p>",
                    IsHtml = true,
                },
                cancellationToken: cancellationToken
            );
        }

        // 3. Publish user event for other services to subscribe
        await _integrationEventBus.PublishAsync(
            new UserEventIntegrationEvent
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                EventType = "UserCreated",
                EventData = new Dictionary<string, object> { ["Email"] = notification.Email ?? "" },
            },
            cancellationToken: cancellationToken
        );
    }
}

/// <summary>
/// User login event handler
/// </summary>
public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<UserLoggedInEventHandler> _logger;

    public UserLoggedInEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<UserLoggedInEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        var action = notification.Success ? "LoginSuccess" : "LoginFailed";

        _logger.LogInformation(
            "Handling UserLoggedInEvent - UserId: {UserId}, Success: {Success}",
            notification.UserId,
            notification.Success
        );

        // Publish audit log
        await _integrationEventBus.PublishAsync(
            new AuditLogIntegrationEvent
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                Action = action,
                EntityType = "User",
                EntityId = notification.UserId.ToString(),
                Description = notification.Success
                    ? $"User '{notification.UserName}' logged in successfully"
                    : $"Login failed for user '{notification.UserName}': {notification.FailureReason}",
                IpAddress = notification.IpAddress,
                UserAgent = notification.UserAgent,
            },
            cancellationToken: cancellationToken
        );
    }
}

/// <summary>
/// Password change event handler
/// </summary>
public class UserPasswordChangedEventHandler : INotificationHandler<UserPasswordChangedEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<UserPasswordChangedEventHandler> _logger;

    public UserPasswordChangedEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<UserPasswordChangedEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(
        UserPasswordChangedEvent notification,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "Handling UserPasswordChangedEvent - UserId: {UserId}",
            notification.UserId
        );

        // Publish audit log
        await _integrationEventBus.PublishAsync(
            new AuditLogIntegrationEvent
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                Action = "PasswordChanged",
                EntityType = "User",
                EntityId = notification.UserId.ToString(),
                Description = $"Password changed for user '{notification.UserName}'",
                IpAddress = notification.IpAddress,
            },
            cancellationToken: cancellationToken
        );

        // Invalidate user session cache
        await _integrationEventBus.PublishAsync(
            new CacheInvalidationIntegrationEvent
            {
                Pattern = $"user:session:{notification.UserId}:*",
                Reason = "Password changed",
            },
            cancellationToken: cancellationToken
        );
    }
}

/// <summary>
/// Role assignment event handler
/// </summary>
public class RoleAssignedEventHandler : INotificationHandler<RoleAssignedEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<RoleAssignedEventHandler> _logger;

    public RoleAssignedEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<RoleAssignedEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(RoleAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling RoleAssignedEvent - UserId: {UserId}, Role: {RoleName}",
            notification.UserId,
            notification.RoleName
        );

        // Publish audit log
        await _integrationEventBus.PublishAsync(
            new AuditLogIntegrationEvent
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                Action = "RoleAssigned",
                EntityType = "UserRole",
                EntityId = notification.UserId.ToString(),
                Description =
                    $"Role '{notification.RoleName}' assigned to user '{notification.UserName}'",
            },
            cancellationToken: cancellationToken
        );

        // Invalidate permissions cache
        await _integrationEventBus.PublishAsync(
            new CacheInvalidationIntegrationEvent
            {
                Pattern = $"user:permissions:{notification.UserId}:*",
                Reason = "Role assigned",
            },
            cancellationToken: cancellationToken
        );
    }
}

/// <summary>
/// Configuration change event handler
/// </summary>
public class ConfigurationChangedEventHandler : INotificationHandler<ConfigurationChangedEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<ConfigurationChangedEventHandler> _logger;

    public ConfigurationChangedEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<ConfigurationChangedEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(
        ConfigurationChangedEvent notification,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "Handling ConfigurationChangedEvent - Group: {Group}, Key: {Key}",
            notification.ConfigGroup,
            notification.ConfigKey
        );

        // Notify all instances of configuration change
        await _integrationEventBus.PublishAsync(
            new ConfigChangedIntegrationEvent
            {
                ConfigGroup = notification.ConfigGroup,
                ConfigKey = notification.ConfigKey,
                OldValue = notification.OldValue,
                NewValue = notification.NewValue,
                ChangedBy = notification.ChangedBy?.ToString(),
            },
            cancellationToken: cancellationToken
        );
    }
}

/// <summary>
/// Alert triggered event handler
/// </summary>
public class AlertTriggeredEventHandler : INotificationHandler<AlertTriggeredEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<AlertTriggeredEventHandler> _logger;

    public AlertTriggeredEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<AlertTriggeredEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(AlertTriggeredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Alert triggered - Rule: {RuleName}, Metric: {MetricType}, Value: {Value}, Threshold: {Threshold}",
            notification.RuleName,
            notification.MetricType,
            notification.CurrentValue,
            notification.Threshold
        );

        // Publish alert notification
        await _integrationEventBus.PublishAsync(
            new AlertNotificationIntegrationEvent
            {
                AlertRuleId = notification.AlertRuleId,
                RuleName = notification.RuleName,
                MetricType = notification.MetricType,
                Severity = notification.Severity,
                CurrentValue = notification.CurrentValue,
                Threshold = notification.Threshold,
                Description =
                    $"{notification.MetricType} exceeded threshold: {notification.CurrentValue} > {notification.Threshold}",
            },
            cancellationToken: cancellationToken
        );
    }
}

/// <summary>
/// Entity change event handler (general audit)
/// </summary>
public class EntityChangedEventHandler : INotificationHandler<EntityChangedEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<EntityChangedEventHandler> _logger;

    public EntityChangedEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<EntityChangedEventHandler> logger
    )
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task Handle(EntityChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Entity changed - Type: {EntityType}, Id: {EntityId}, Action: {Action}",
            notification.EntityType,
            notification.EntityId,
            notification.Action
        );

        // Publish audit log
        await _integrationEventBus.PublishAsync(
            new AuditLogIntegrationEvent
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                Action = notification.Action,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId.ToString(),
                OldValues = notification.OldValues,
                NewValues = notification.NewValues,
            },
            cancellationToken: cancellationToken
        );
    }
}
