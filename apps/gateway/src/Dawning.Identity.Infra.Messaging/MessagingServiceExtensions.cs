using Dawning.Identity.Application.Interfaces.Distributed;
using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Application.Interfaces.Messaging;
using Dawning.Identity.Infra.Messaging.Distributed;
using Dawning.Identity.Infra.Messaging.Kafka;
using Dawning.Identity.Infra.Messaging.Kafka.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Dawning.Identity.Infra.Messaging;

/// <summary>
/// Messaging service dependency injection extensions
/// </summary>
public static class MessagingServiceExtensions
{
    /// <summary>
    /// Add complete messaging services (MediatR + Kafka)
    /// </summary>
    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure Kafka options
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        // Register MediatR domain event dispatcher
        services.AddMediatRDomainEventDispatcher();

        // Register integration event bus (Kafka)
        services.AddKafkaIntegrationEventBus();

        // Register distributed lock
        services.AddDistributedLock();

        return services;
    }

    /// <summary>
    /// Add MediatR domain event dispatcher
    /// </summary>
    public static IServiceCollection AddMediatRDomainEventDispatcher(
        this IServiceCollection services
    )
    {
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        return services;
    }

    /// <summary>
    /// Add Kafka integration event bus
    /// </summary>
    public static IServiceCollection AddKafkaIntegrationEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventBus>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>();
            var logger = sp.GetRequiredService<ILogger<KafkaIntegrationEventBus>>();
            var nullLogger = sp.GetRequiredService<ILogger<NullIntegrationEventBus>>();

            if (!options.Value.Enabled)
            {
                return new NullIntegrationEventBus(nullLogger);
            }

            try
            {
                return new KafkaIntegrationEventBus(options, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to create Kafka integration event bus, using null bus"
                );
                return new NullIntegrationEventBus(nullLogger);
            }
        });

        return services;
    }

    /// <summary>
    /// Add Kafka integration event consumers
    /// </summary>
    public static IServiceCollection AddKafkaIntegrationEventConsumers(
        this IServiceCollection services
    )
    {
        services.AddHostedService<AuditLogIntegrationEventConsumer>();
        services.AddHostedService<AlertNotificationIntegrationEventConsumer>();
        services.AddHostedService<EmailIntegrationEventConsumer>();
        services.AddHostedService<ConfigChangedIntegrationEventConsumer>();
        services.AddHostedService<CacheInvalidationIntegrationEventConsumer>();
        services.AddHostedService<UserEventIntegrationEventConsumer>();

        return services;
    }

    /// <summary>
    /// Add distributed lock service
    /// </summary>
    public static IServiceCollection AddDistributedLock(this IServiceCollection services)
    {
        services.AddSingleton<IDistributedLock>(sp =>
        {
            var redis = sp.GetService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<ILogger<RedisDistributedLock>>();
            return new RedisDistributedLock(redis, logger);
        });

        return services;
    }
}
