using Dawning.Identity.Application.Dtos.IntegrationEvents;
using Dawning.Identity.Application.Interfaces.Caching;
using Dawning.Identity.Application.Interfaces.Messaging;
using Dawning.Identity.Application.Interfaces.Notification;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Identity.Infra.Messaging.Kafka.Consumers;

/// <summary>
/// Audit log integration event consumer
/// Consumes audit log events from Kafka and persists them to the database
/// </summary>
public class AuditLogIntegrationEventConsumer
    : KafkaIntegrationEventConsumer<AuditLogIntegrationEvent>
{
    public AuditLogIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<AuditLogIntegrationEventConsumer> logger
    )
        : base(scopeFactory, options, logger, options.Value.Topics.AuditLog) { }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        AuditLogIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = @event.UserId,
            Username = @event.UserName,
            Action = @event.Action,
            EntityType = @event.EntityType,
            EntityId =
                string.IsNullOrEmpty(@event.EntityId) ? null
                : Guid.TryParse(@event.EntityId, out var eid) ? eid
                : null,
            Description = @event.Description,
            OldValues = @event.OldValues,
            NewValues = @event.NewValues,
            IpAddress = @event.IpAddress,
            UserAgent = @event.UserAgent,
            RequestPath = @event.RequestPath,
            RequestMethod = @event.RequestMethod,
            StatusCode = @event.StatusCode,
            CreatedAt = @event.OccurredOn,
            Timestamp = new DateTimeOffset(@event.OccurredOn).ToUnixTimeMilliseconds(),
        };

        await unitOfWork.AuditLog.InsertAsync(auditLog);
        await unitOfWork.CommitAsync();
    }
}

/// <summary>
/// Alert notification integration event consumer
/// </summary>
public class AlertNotificationIntegrationEventConsumer
    : KafkaIntegrationEventConsumer<AlertNotificationIntegrationEvent>
{
    private readonly ILogger<AlertNotificationIntegrationEventConsumer> _logger;

    public AlertNotificationIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<AlertNotificationIntegrationEventConsumer> logger
    )
        : base(scopeFactory, options, logger, options.Value.Topics.AlertNotification)
    {
        _logger = logger;
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        AlertNotificationIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var notificationService = serviceProvider.GetService<IRealTimeNotificationService>();

        if (notificationService != null)
        {
            var alert = new RealTimeAlertNotification
            {
                Type = "alert",
                Title = @event.RuleName,
                Message = @event.Description ?? $"{@event.MetricType} alert triggered",
                Severity = @event.Severity,
                RuleName = @event.RuleName,
                MetricType = @event.MetricType,
                Value = (decimal)@event.CurrentValue,
                Threshold = (decimal)@event.Threshold,
            };

            await notificationService.SendAlertNotificationAsync(alert);
        }

        _logger.LogWarning(
            "Alert triggered - Rule: {RuleName}, Metric: {MetricType}, Value: {Value}, Threshold: {Threshold}",
            @event.RuleName,
            @event.MetricType,
            @event.CurrentValue,
            @event.Threshold
        );
    }
}

/// <summary>
/// Email integration event consumer
/// </summary>
public class EmailIntegrationEventConsumer : KafkaIntegrationEventConsumer<EmailIntegrationEvent>
{
    private readonly ILogger<EmailIntegrationEventConsumer> _logger;

    public EmailIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<EmailIntegrationEventConsumer> logger
    )
        : base(scopeFactory, options, logger, options.Value.Topics.Email)
    {
        _logger = logger;
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        EmailIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        // TODO: Enable when IEmailService is implemented
        _logger.LogInformation(
            "Email queued for {To}. Subject: {Subject} (Email service not implemented yet)",
            @event.To,
            @event.Subject
        );

        await Task.CompletedTask;
    }
}

/// <summary>
/// Configuration change integration event consumer
/// </summary>
public class ConfigChangedIntegrationEventConsumer
    : KafkaIntegrationEventConsumer<ConfigChangedIntegrationEvent>
{
    private readonly ILogger<ConfigChangedIntegrationEventConsumer> _logger;

    public ConfigChangedIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<ConfigChangedIntegrationEventConsumer> logger
    )
        : base(scopeFactory, options, logger, options.Value.Topics.ConfigChanged)
    {
        _logger = logger;
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        ConfigChangedIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var cacheService = serviceProvider.GetService<ICacheService>();

        if (cacheService != null)
        {
            var cacheKey = $"config:{@event.ConfigGroup}:{@event.ConfigKey}";
            await cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation(
                "Configuration cache invalidated. Group: {Group}, Key: {Key}, ChangedBy: {ChangedBy}",
                @event.ConfigGroup,
                @event.ConfigKey,
                @event.ChangedBy
            );
        }
    }
}

/// <summary>
/// Cache invalidation integration event consumer
/// </summary>
public class CacheInvalidationIntegrationEventConsumer
    : KafkaIntegrationEventConsumer<CacheInvalidationIntegrationEvent>
{
    private readonly ILogger<CacheInvalidationIntegrationEventConsumer> _logger;

    public CacheInvalidationIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<CacheInvalidationIntegrationEventConsumer> logger
    )
        : base(scopeFactory, options, logger, options.Value.Topics.CacheInvalidation)
    {
        _logger = logger;
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        CacheInvalidationIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var cacheService = serviceProvider.GetService<ICacheService>();

        if (cacheService == null)
            return;

        if (!string.IsNullOrEmpty(@event.Pattern))
        {
            await cacheService.RemoveByPrefixAsync(@event.Pattern);
            _logger.LogDebug("Cache invalidated by pattern: {Pattern}", @event.Pattern);
        }
        else
        {
            await cacheService.RemoveAsync(@event.CacheKey);
            _logger.LogDebug("Cache invalidated: {Key}", @event.CacheKey);
        }
    }
}

/// <summary>
/// User event integration event consumer
/// </summary>
public class UserEventIntegrationEventConsumer
    : KafkaIntegrationEventConsumer<UserEventIntegrationEvent>
{
    private readonly ILogger<UserEventIntegrationEventConsumer> _logger;

    public UserEventIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<UserEventIntegrationEventConsumer> logger
    )
        : base(scopeFactory, options, logger, options.Value.Topics.UserEvent)
    {
        _logger = logger;
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        UserEventIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "User event received - Type: {EventType}, UserId: {UserId}, UserName: {UserName}",
            @event.EventType,
            @event.UserId,
            @event.UserName
        );

        // Handle based on event type
        switch (@event.EventType)
        {
            case "RoleAssigned":
            case "RoleRevoked":
                // Invalidate permissions cache
                var cacheService = serviceProvider.GetService<ICacheService>();
                if (cacheService != null)
                {
                    await cacheService.RemoveByPrefixAsync($"user:permissions:{@event.UserId}:");
                }
                break;
        }
    }
}
