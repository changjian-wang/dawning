using Dawning.Identity.Application.Interfaces.Messaging;
using Dawning.Identity.Application.Services.Messaging;
using Dawning.Identity.Application.Services.Messaging.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Identity.Application.Extensions;

/// <summary>
/// Kafka 消息队列服务注册扩展
/// </summary>
public static class KafkaServiceExtensions
{
    /// <summary>
    /// 添加 Kafka 消息队列服务
    /// </summary>
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 绑定配置
        var kafkaSection = configuration.GetSection(KafkaOptions.SectionName);
        services.Configure<KafkaOptions>(kafkaSection);

        var options = kafkaSection.Get<KafkaOptions>() ?? new KafkaOptions();

        if (options.Enabled)
        {
            // 注册生产者（单例，线程安全）
            services.AddSingleton<IMessageBus, KafkaMessageBus>();

            // 注册消费者后台服务
            services.AddHostedService<AuditLogConsumer>();
            services.AddHostedService<AlertNotificationConsumer>();
            services.AddHostedService<EmailConsumer>();
            services.AddHostedService<ConfigChangedConsumer>();
            services.AddHostedService<CacheInvalidationConsumer>();
            services.AddHostedService<UserEventConsumer>();
        }
        else
        {
            // Kafka 禁用时使用空实现
            services.AddSingleton<IMessageBus, NullMessageBus>();
        }

        return services;
    }

    /// <summary>
    /// 仅添加 Kafka 生产者（不启动消费者）
    /// </summary>
    public static IServiceCollection AddKafkaProducer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var kafkaSection = configuration.GetSection(KafkaOptions.SectionName);
        services.Configure<KafkaOptions>(kafkaSection);

        var options = kafkaSection.Get<KafkaOptions>() ?? new KafkaOptions();

        if (options.Enabled)
        {
            services.AddSingleton<IMessageBus, KafkaMessageBus>();
        }
        else
        {
            services.AddSingleton<IMessageBus, NullMessageBus>();
        }

        return services;
    }
}
