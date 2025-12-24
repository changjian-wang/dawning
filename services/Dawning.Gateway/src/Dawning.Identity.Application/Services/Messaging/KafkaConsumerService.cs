using System.Text.Json;
using Confluent.Kafka;
using Dawning.Identity.Application.Interfaces.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Identity.Application.Services.Messaging;

/// <summary>
/// Kafka 消费者后台服务基类
/// </summary>
/// <typeparam name="TMessage">消息类型</typeparam>
public abstract class KafkaConsumerService<TMessage> : BackgroundService
    where TMessage : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger _logger;
    private readonly string _topic;
    private IConsumer<string, string>? _consumer;

    protected KafkaConsumerService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger logger,
        string topic
    )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _topic = topic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Kafka is disabled. Consumer for topic {Topic} will not start.",
                _topic
            );
            return;
        }

        await Task.Yield(); // 让启动继续

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            EnableAutoCommit = _options.Consumer.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_options.Consumer.AutoOffsetReset, true),
            SessionTimeoutMs = _options.Consumer.SessionTimeoutMs,
            HeartbeatIntervalMs = _options.Consumer.HeartbeatIntervalMs,
            MaxPollIntervalMs = 300000,
            FetchWaitMaxMs = _options.Consumer.FetchWaitMaxMs,
            // 分布式环境下的配置
            ClientId = $"{GetType().Name}-{Environment.MachineName}",
            // 容错配置
            EnablePartitionEof = false,
            IsolationLevel = IsolationLevel.ReadCommitted,
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler(
                (_, error) =>
                {
                    _logger.LogError(
                        "Kafka Consumer Error: {Reason} (Code: {Code})",
                        error.Reason,
                        error.Code
                    );
                }
            )
            .SetPartitionsAssignedHandler(
                (_, partitions) =>
                {
                    _logger.LogInformation(
                        "Assigned partitions: [{Partitions}]",
                        string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}"))
                    );
                }
            )
            .SetPartitionsRevokedHandler(
                (_, partitions) =>
                {
                    _logger.LogInformation(
                        "Revoked partitions: [{Partitions}]",
                        string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}"))
                    );
                }
            )
            .Build();

        _consumer.Subscribe(_topic);
        _logger.LogInformation(
            "Kafka Consumer started. Topic: {Topic}, Group: {Group}",
            _topic,
            _options.ConsumerGroupId
        );

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result?.Message?.Value == null)
                        continue;

                    await ProcessMessageAsync(result, stoppingToken);

                    // 手动提交偏移量
                    if (!_options.Consumer.EnableAutoCommit)
                    {
                        _consumer.Commit(result);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from topic {Topic}", _topic);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error processing message from topic {Topic}",
                        _topic
                    );
                    await Task.Delay(1000, stoppingToken); // 错误后等待
                }
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _logger.LogInformation("Kafka Consumer stopped. Topic: {Topic}", _topic);
        }
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var message = JsonSerializer.Deserialize<TMessage>(
                result.Message.Value,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                }
            );

            if (message == null)
            {
                _logger.LogWarning("Failed to deserialize message from topic {Topic}", _topic);
                return;
            }

            // 使用新的作用域处理消息
            using var scope = _scopeFactory.CreateScope();
            await HandleMessageAsync(scope.ServiceProvider, message, cancellationToken);

            _logger.LogDebug(
                "Message processed from topic {Topic}. Key: {Key}, Partition: {Partition}, Offset: {Offset}",
                _topic,
                result.Message.Key,
                result.Partition.Value,
                result.Offset.Value
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize message from topic {Topic}. Value: {Value}",
                _topic,
                result.Message.Value.Substring(0, Math.Min(200, result.Message.Value.Length))
            );
        }
    }

    /// <summary>
    /// 处理消息（子类实现）
    /// </summary>
    protected abstract Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        TMessage message,
        CancellationToken cancellationToken
    );
}
