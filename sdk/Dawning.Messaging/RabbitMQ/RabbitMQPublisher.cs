using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Dawning.Messaging.RabbitMQ;

/// <summary>
/// RabbitMQ 消息发布者
/// </summary>
public class RabbitMQPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly MessagingOptions _options;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// 初始化 RabbitMQ 发布者
    /// </summary>
    public RabbitMQPublisher(IOptions<MessagingOptions> options, ILogger<RabbitMQPublisher> logger)
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
        await EnsureConnectionAsync(cancellationToken);

        var actualRoutingKey = routingKey ?? typeof(T).Name.ToLowerInvariant();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOptions));

        var properties = new BasicProperties
        {
            Persistent = _options.RabbitMQ.Durable,
            ContentType = "application/json",
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
        };

        await _channel!.BasicPublishAsync(
            exchange: _options.DefaultExchange,
            routingKey: actualRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );

        _logger.LogDebug(
            "Published message to {Exchange}/{RoutingKey}",
            _options.DefaultExchange,
            actualRoutingKey
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
        foreach (var message in messages)
        {
            await PublishAsync(message, routingKey, cancellationToken);
        }
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
            "RabbitMQ connection established to {Host}:{Port}",
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
