using System.Text.Json;
using Confluent.Kafka;
using Dawning.Identity.Application.Interfaces.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Identity.Application.Services.Messaging;

/// <summary>
/// Kafka 消息总线实现
/// </summary>
public class KafkaMessageBus : IMessageBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaMessageBus> _logger;
    private readonly string _serviceName;
    private bool _disposed;

    public KafkaMessageBus(
        IOptions<KafkaOptions> options,
        ILogger<KafkaMessageBus> logger)
    {
        _options = options.Value;
        _logger = logger;
        _serviceName = "Dawning.Identity.Api";

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Enum.Parse<Acks>(_options.Producer.Acks, true),
            EnableIdempotence = _options.Producer.EnableIdempotence,
            MessageMaxBytes = _options.Producer.MaxMessageBytes,
            MessageTimeoutMs = _options.Producer.MessageTimeoutMs,
            MessageSendMaxRetries = _options.Producer.Retries,
            LingerMs = _options.Producer.LingerMs,
            BatchSize = _options.Producer.BatchSize,
            CompressionType = Enum.Parse<CompressionType>(_options.Producer.CompressionType, true),
            // 分布式环境下的配置
            ClientId = $"{_serviceName}-{Environment.MachineName}",
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka Producer Error: {Reason} (Code: {Code})", error.Reason, error.Code);
            })
            .SetLogHandler((_, log) =>
            {
                _logger.LogDebug("Kafka Producer: {Message}", log.Message);
            })
            .Build();

        _logger.LogInformation("Kafka Producer initialized. Brokers: {Brokers}", _options.BootstrapServers);
    }

    /// <summary>
    /// 发布消息到指定主题
    /// </summary>
    public async Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Kafka is disabled. Message to topic {Topic} was not sent.", topic);
            return;
        }

        try
        {
            // 设置消息元数据
            if (message is MessageBase baseMessage)
            {
                baseMessage.SourceService ??= _serviceName;
            }

            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var kafkaMessage = new Message<string, string>
            {
                Key = key ?? Guid.NewGuid().ToString(),
                Value = json,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

            _logger.LogDebug(
                "Message published to {Topic}. Partition: {Partition}, Offset: {Offset}",
                topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
            throw;
        }
    }

    /// <summary>
    /// 发布消息（使用消息类型名作为主题）
    /// </summary>
    public Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        var topic = GetTopicForMessageType<T>();
        return PublishAsync(topic, message, key, cancellationToken);
    }

    /// <summary>
    /// 根据消息类型获取主题名
    /// </summary>
    private string GetTopicForMessageType<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(AuditLogMessage) => _options.Topics.AuditLog,
            nameof(AlertNotificationMessage) => _options.Topics.AlertNotification,
            nameof(EmailMessage) => _options.Topics.Email,
            nameof(UserEventMessage) => _options.Topics.UserEvent,
            nameof(SystemEventMessage) => _options.Topics.SystemEvent,
            nameof(ConfigChangedMessage) => _options.Topics.ConfigChanged,
            nameof(CacheInvalidationMessage) => _options.Topics.CacheInvalidation,
            _ => $"dawning.{typeof(T).Name.ToLowerInvariant()}"
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
            _logger.LogInformation("Kafka Producer disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing Kafka Producer");
        }
    }
}

/// <summary>
/// 空消息总线实现（Kafka 禁用时使用）
/// </summary>
public class NullMessageBus : IMessageBus
{
    private readonly ILogger<NullMessageBus> _logger;

    public NullMessageBus(ILogger<NullMessageBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug("NullMessageBus: Message to topic {Topic} was not sent (Kafka disabled)", topic);
        return Task.CompletedTask;
    }

    public Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug("NullMessageBus: Message of type {Type} was not sent (Kafka disabled)", typeof(T).Name);
        return Task.CompletedTask;
    }
}
