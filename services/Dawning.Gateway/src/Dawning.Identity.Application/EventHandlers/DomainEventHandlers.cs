using Dawning.Identity.Application.Dtos.IntegrationEvents;
using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.EventHandlers;

/// <summary>
/// 用户创建事件处理器
/// 进程内处理：记录审计日志、发送欢迎邮件等
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

        // 1. 发布审计日志到 Kafka
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

        // 2. 如果有邮箱，发送欢迎邮件
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

        // 3. 发布用户事件供其他服务订阅
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
/// 用户登录事件处理器
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

        // 发布审计日志
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
/// 密码变更事件处理器
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

        // 发布审计日志
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

        // 失效用户会话缓存
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
/// 角色分配事件处理器
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

        // 发布审计日志
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

        // 失效权限缓存
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
/// 配置变更事件处理器
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

        // 通知所有实例配置变更
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
/// 告警触发事件处理器
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

        // 发布告警通知
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
/// 实体变更事件处理器（通用审计）
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

        // 发布审计日志
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
