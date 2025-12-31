using Dawning.Messaging.AzureServiceBus;
using Dawning.Messaging.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Messaging;

/// <summary>
/// Messaging service extension methods
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Adds Dawning messaging services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
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
    /// Adds RabbitMQ messaging services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
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
    /// Adds Azure Service Bus messaging services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Connection string</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
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
