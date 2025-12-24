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
/// 消息服务依赖注入扩展
/// </summary>
public static class MessagingServiceExtensions
{
    /// <summary>
    /// 添加完整的消息服务（MediatR + Kafka）
    /// </summary>
    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置 Kafka 选项
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        // 注册 MediatR 领域事件分发器
        services.AddMediatRDomainEventDispatcher();

        // 注册集成事件总线 (Kafka)
        services.AddKafkaIntegrationEventBus();

        // 注册分布式锁
        services.AddDistributedLock();

        return services;
    }

    /// <summary>
    /// 添加 MediatR 领域事件分发器
    /// </summary>
    public static IServiceCollection AddMediatRDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        return services;
    }

    /// <summary>
    /// 添加 Kafka 集成事件总线
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
                logger.LogWarning(ex, "Failed to create Kafka integration event bus, using null bus");
                return new NullIntegrationEventBus(nullLogger);
            }
        });

        return services;
    }

    /// <summary>
    /// 添加 Kafka 集成事件消费者
    /// </summary>
    public static IServiceCollection AddKafkaIntegrationEventConsumers(this IServiceCollection services)
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
    /// 添加分布式锁服务
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
