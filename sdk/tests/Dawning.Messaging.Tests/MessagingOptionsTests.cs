using FluentAssertions;
using Xunit;

namespace Dawning.Messaging.Tests;

public class MessagingOptionsTests
{
    [Fact]
    public void MessagingOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new MessagingOptions();

        // Assert
        options.Provider.Should().Be(MessagingProvider.RabbitMQ);
        options.DefaultExchange.Should().Be("dawning.events");
        options.SerializationFormat.Should().Be(SerializationFormat.Json);
    }

    [Fact]
    public void RabbitMQOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new RabbitMQOptions();

        // Assert
        options.HostName.Should().Be("localhost");
        options.Port.Should().Be(5672);
        options.UserName.Should().Be("guest");
        options.Password.Should().Be("guest");
        options.VirtualHost.Should().Be("/");
        options.ExchangeType.Should().Be("topic");
        options.Durable.Should().BeTrue();
        options.PrefetchCount.Should().Be(10);
    }

    [Fact]
    public void ServiceBusOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ServiceBusOptions();

        // Assert
        options.ConnectionString.Should().BeEmpty();
        options.TopicName.Should().Be("dawning-events");
        options.SubscriptionName.Should().Be("default");
        options.MaxConcurrentCalls.Should().Be(10);
        options.AutoCompleteMessages.Should().BeTrue();
    }

    [Fact]
    public void MessagingOptions_ShouldBeConfigurable()
    {
        // Arrange
        var options = new MessagingOptions
        {
            Provider = MessagingProvider.AzureServiceBus,
            DefaultExchange = "custom-exchange",
        };

        // Assert
        options.Provider.Should().Be(MessagingProvider.AzureServiceBus);
        options.DefaultExchange.Should().Be("custom-exchange");
    }
}
