using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Dawning.Messaging.RabbitMQ;

/// <summary>
/// RabbitMQ 消息订阅者
/// </summary>
public class RabbitMQSubscriber : IMessageSubscriber, IAsyncDisposable
{
    private readonly MessagingOptions _options;
    private readonly ILogger<RabbitMQSubscriber> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly Dictionary<string, string> _consumerTags = new();
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// 初始化 RabbitMQ 订阅者
    /// </summary>
    public RabbitMQSubscriber(
        IOptions<MessagingOptions> options,
        ILogger<RabbitMQSubscriber> logger
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
        await EnsureConnectionAsync(cancellationToken);

        var queueName =
            subscriptionName ?? $"{_options.DefaultExchange}.{typeof(T).Name.ToLowerInvariant()}";
        var routingKey = typeof(T).Name.ToLowerInvariant();

        await _channel!.QueueDeclareAsync(
            queue: queueName,
            durable: _options.RabbitMQ.Durable,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.DefaultExchange,
            routingKey: routingKey,
            cancellationToken: cancellationToken
        );

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.RabbitMQ.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(json, JsonOptions);

                if (message is not null)
                {
                    await handler(message, cancellationToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                _logger.LogDebug("Processed message from queue {Queue}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {Queue}", queueName);
                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken
                );
            }
        };

        var consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        _consumerTags[queueName] = consumerTag;
        _logger.LogInformation(
            "Subscribed to queue {Queue} with routing key {RoutingKey}",
            queueName,
            routingKey
        );
    }

    /// <inheritdoc />
    public async Task UnsubscribeAsync(
        string subscriptionName,
        CancellationToken cancellationToken = default
    )
    {
        if (_channel is null || !_consumerTags.TryGetValue(subscriptionName, out var consumerTag))
        {
            return;
        }

        await _channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
        _consumerTags.Remove(subscriptionName);
        _logger.LogInformation("Unsubscribed from queue {Queue}", subscriptionName);
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (
            _connection is not null
            && _connection.IsOpen
            && _channel is not null
            && _channel.IsOpen
        )
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.RabbitMQ.HostName,
            Port = _options.RabbitMQ.Port,
            UserName = _options.RabbitMQ.UserName,
            Password = _options.RabbitMQ.Password,
            VirtualHost = _options.RabbitMQ.VirtualHost,
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.DefaultExchange,
            type: _options.RabbitMQ.ExchangeType,
            durable: _options.RabbitMQ.Durable,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "RabbitMQ subscriber connection established to {Host}:{Port}",
            _options.RabbitMQ.HostName,
            _options.RabbitMQ.Port
        );
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
