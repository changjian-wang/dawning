using Dawning.Messaging.AzureServiceBus;
using Dawning.Messaging.RabbitMQ;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dawning.Messaging.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDawningRabbitMQ_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddDawningRabbitMQ(options =>
        {
            options.HostName = "localhost";
            options.Port = 5672;
        });

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetService<IMessagePublisher>();
        var subscriber = provider.GetService<IMessageSubscriber>();

        // Assert
        publisher.Should().NotBeNull();
        publisher.Should().BeOfType<RabbitMQPublisher>();
        subscriber.Should().NotBeNull();
        subscriber.Should().BeOfType<RabbitMQSubscriber>();
    }

    [Fact]
    public void AddDawningServiceBus_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddDawningServiceBus(
            "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            options =>
            {
                options.TopicName = "test-topic";
            }
        );

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetService<IMessagePublisher>();
        var subscriber = provider.GetService<IMessageSubscriber>();

        // Assert
        publisher.Should().NotBeNull();
        publisher.Should().BeOfType<ServiceBusPublisher>();
        subscriber.Should().NotBeNull();
        subscriber.Should().BeOfType<ServiceBusSubscriber>();
    }

    [Fact]
    public void AddDawningMessaging_WithRabbitMQ_ShouldConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddDawningMessaging(options =>
        {
            options.Provider = MessagingProvider.RabbitMQ;
            options.DefaultExchange = "my-exchange";
            options.RabbitMQ.HostName = "rabbitmq.local";
            options.RabbitMQ.Port = 5673;
        });

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetService<IMessagePublisher>();

        // Assert
        publisher.Should().BeOfType<RabbitMQPublisher>();
    }
}
