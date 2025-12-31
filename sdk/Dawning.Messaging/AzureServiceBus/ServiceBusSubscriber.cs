using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus message subscriber
/// </summary>
public class ServiceBusSubscriber : IMessageSubscriber, IAsyncDisposable
{
    private readonly MessagingOptions _options;
    private readonly ILogger<ServiceBusSubscriber> _logger;
    private ServiceBusClient? _client;
    private readonly Dictionary<string, ServiceBusProcessor> _processors = new();
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes the Azure Service Bus subscriber
    /// </summary>
    public ServiceBusSubscriber(
        IOptions<MessagingOptions> options,
        ILogger<ServiceBusSubscriber> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SubscribeAsync<T>(
        Func<T, CancellationToken, Task> handler,
        string? subscriptionName = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        EnsureConnection();

        var actualSubscriptionName = subscriptionName ?? _options.ServiceBus.SubscriptionName;

        var processor = _client!.CreateProcessor(
            _options.ServiceBus.TopicName,
            actualSubscriptionName,
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = _options.ServiceBus.MaxConcurrentCalls,
                AutoCompleteMessages = _options.ServiceBus.AutoCompleteMessages,
            }
        );

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var body = args.Message.Body.ToString();
                var message = JsonSerializer.Deserialize<T>(body, JsonOptions);

                if (message is not null)
                {
                    await handler(message, cancellationToken);
                }

                if (!_options.ServiceBus.AutoCompleteMessages)
                {
                    await args.CompleteMessageAsync(args.Message, cancellationToken);
                }

                _logger.LogDebug(
                    "Processed message from {Topic}/{Subscription}",
                    _options.ServiceBus.TopicName,
                    actualSubscriptionName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing message from {Topic}/{Subscription}",
                    _options.ServiceBus.TopicName,
                    actualSubscriptionName
                );
                throw;
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(
                args.Exception,
                "Error in Service Bus processor for {Topic}/{Subscription}",
                _options.ServiceBus.TopicName,
                actualSubscriptionName
            );
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(cancellationToken);
        _processors[actualSubscriptionName] = processor;

        _logger.LogInformation(
            "Subscribed to {Topic}/{Subscription}",
            _options.ServiceBus.TopicName,
            actualSubscriptionName
        );
    }

    /// <inheritdoc />
    public async Task UnsubscribeAsync(
        string subscriptionName,
        CancellationToken cancellationToken = default
    )
    {
        if (!_processors.TryGetValue(subscriptionName, out var processor))
        {
            return;
        }

        await processor.StopProcessingAsync(cancellationToken);
        await processor.DisposeAsync();
        _processors.Remove(subscriptionName);

        _logger.LogInformation(
            "Unsubscribed from {Topic}/{Subscription}",
            _options.ServiceBus.TopicName,
            subscriptionName
        );
    }

    private void EnsureConnection()
    {
        if (_client is not null)
        {
            return;
        }

        _client = new ServiceBusClient(_options.ServiceBus.ConnectionString);
        _logger.LogInformation(
            "Azure Service Bus client created for {Topic}",
            _options.ServiceBus.TopicName
        );
    }

    /// <summary>
    /// Releases resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        foreach (var processor in _processors.Values)
        {
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }
        _processors.Clear();

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
