using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus 消息发布者
/// </summary>
public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly MessagingOptions _options;
    private readonly ILogger<ServiceBusPublisher> _logger;
    private ServiceBusClient? _client;
    private ServiceBusSender? _sender;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// 初始化 Azure Service Bus 发布者
    /// </summary>
    public ServiceBusPublisher(
        IOptions<MessagingOptions> options,
        ILogger<ServiceBusPublisher> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync<T>(
        T message,
        string? routingKey = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        EnsureConnection();

        var messageType = typeof(T).Name;
        var body = JsonSerializer.Serialize(message, JsonOptions);

        var serviceBusMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = routingKey ?? messageType,
            MessageId = Guid.NewGuid().ToString(),
        };

        serviceBusMessage.ApplicationProperties["MessageType"] = messageType;

        await _sender!.SendMessageAsync(serviceBusMessage, cancellationToken);
        _logger.LogDebug(
            "Published message {MessageType} to {Topic}",
            messageType,
            _options.ServiceBus.TopicName
        );
    }

    /// <inheritdoc />
    public async Task PublishBatchAsync<T>(
        IEnumerable<T> messages,
        string? routingKey = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        EnsureConnection();

        var messageType = typeof(T).Name;
        using var batch = await _sender!.CreateMessageBatchAsync(cancellationToken);

        foreach (var message in messages)
        {
            var body = JsonSerializer.Serialize(message, JsonOptions);
            var serviceBusMessage = new ServiceBusMessage(body)
            {
                ContentType = "application/json",
                Subject = routingKey ?? messageType,
                MessageId = Guid.NewGuid().ToString(),
            };
            serviceBusMessage.ApplicationProperties["MessageType"] = messageType;

            if (!batch.TryAddMessage(serviceBusMessage))
            {
                // Batch is full, send it and create a new one
                await _sender.SendMessagesAsync(batch, cancellationToken);
                _logger.LogDebug(
                    "Sent batch of messages to {Topic}",
                    _options.ServiceBus.TopicName
                );
            }
        }

        if (batch.Count > 0)
        {
            await _sender.SendMessagesAsync(batch, cancellationToken);
            _logger.LogDebug(
                "Sent final batch of {Count} messages to {Topic}",
                batch.Count,
                _options.ServiceBus.TopicName
            );
        }
    }

    private void EnsureConnection()
    {
        if (_client is not null && _sender is not null)
        {
            return;
        }

        _client = new ServiceBusClient(_options.ServiceBus.ConnectionString);
        _sender = _client.CreateSender(_options.ServiceBus.TopicName);

        _logger.LogInformation(
            "Azure Service Bus connection established to {Topic}",
            _options.ServiceBus.TopicName
        );
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_sender is not null)
        {
            await _sender.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
