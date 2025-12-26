using Dawning.Messaging.AzureServiceBus;
using Dawning.Messaging.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Messaging;

/// <summary>
/// 消息服务扩展方法
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Dawning 消息服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDawningMessaging(
        this IServiceCollection services,
        Action<MessagingOptions>? configure = null
    )
    {
        var options = new MessagingOptions();
        configure?.Invoke(options);

        services.Configure<MessagingOptions>(opt =>
        {
            opt.Provider = options.Provider;
            opt.DefaultExchange = options.DefaultExchange;
            opt.SerializationFormat = options.SerializationFormat;
            opt.RabbitMQ = options.RabbitMQ;
            opt.ServiceBus = options.ServiceBus;
        });

        switch (options.Provider)
        {
            case MessagingProvider.AzureServiceBus:
                services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();
                services.AddSingleton<IMessageSubscriber, ServiceBusSubscriber>();
                break;

            case MessagingProvider.RabbitMQ:
            default:
                services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
                services.AddSingleton<IMessageSubscriber, RabbitMQSubscriber>();
                break;
        }

        return services;
    }

    /// <summary>
    /// 添加 RabbitMQ 消息服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDawningRabbitMQ(
        this IServiceCollection services,
        Action<RabbitMQOptions>? configure = null
    )
    {
        return services.AddDawningMessaging(options =>
        {
            options.Provider = MessagingProvider.RabbitMQ;
            configure?.Invoke(options.RabbitMQ);
        });
    }

    /// <summary>
    /// 添加 Azure Service Bus 消息服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDawningServiceBus(
        this IServiceCollection services,
        string connectionString,
        Action<ServiceBusOptions>? configure = null
    )
    {
        return services.AddDawningMessaging(options =>
        {
            options.Provider = MessagingProvider.AzureServiceBus;
            options.ServiceBus.ConnectionString = connectionString;
            configure?.Invoke(options.ServiceBus);
        });
    }
}
