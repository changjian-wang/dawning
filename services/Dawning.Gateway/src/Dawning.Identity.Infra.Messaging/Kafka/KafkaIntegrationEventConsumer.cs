using System.Text.Json;
using Confluent.Kafka;
using Dawning.Identity.Application.Interfaces.Messaging;
using Dawning.Identity.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Identity.Infra.Messaging.Kafka;

/// <summary>
/// Kafka 集成事件消费者基类
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public abstract class KafkaIntegrationEventConsumer<TEvent> : BackgroundService
    where TEvent : class, IIntegrationEvent
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger _logger;
    private readonly string _topic;
    private readonly JsonSerializerOptions _jsonOptions;
    private IConsumer<string, string>? _consumer;

    protected KafkaIntegrationEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger logger,
        string topic)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _topic = topic;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Kafka consumer for {Topic} is disabled.",
                _topic);
            return;
        }

        await Task.Yield(); // 让启动继续

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_options.AutoOffsetReset, true),
            EnableAutoCommit = _options.EnableAutoCommit,
            SessionTimeoutMs = _options.SessionTimeoutMs,
            HeartbeatIntervalMs = _options.HeartbeatIntervalMs,
            MaxPollIntervalMs = _options.MaxPollIntervalMs
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) =>
            {
                _logger.LogError("Kafka consumer error for {Topic}: {Reason}", _topic, e.Reason);
            })
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation(
                    "Kafka consumer for {Topic} assigned partitions: {Partitions}",
                    _topic, string.Join(", ", partitions.Select(p => p.Partition.Value)));
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                _logger.LogInformation(
                    "Kafka consumer for {Topic} revoked partitions: {Partitions}",
                    _topic, string.Join(", ", partitions.Select(p => p.Partition.Value)));
            })
            .Build();

        _consumer.Subscribe(_topic);

        _logger.LogInformation(
            "Kafka consumer started for topic: {Topic}, GroupId: {GroupId}",
            _topic, _options.ConsumerGroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ConsumeMessageAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常关闭
        }
        finally
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
    }

    private async Task ConsumeMessageAsync(CancellationToken cancellationToken)
    {
        try
        {
            var consumeResult = _consumer?.Consume(TimeSpan.FromSeconds(1));
            if (consumeResult == null) return;

            var @event = JsonSerializer.Deserialize<TEvent>(consumeResult.Message.Value, _jsonOptions);
            if (@event == null)
            {
                _logger.LogWarning(
                    "Failed to deserialize message from {Topic}, Partition: {Partition}, Offset: {Offset}",
                    _topic, consumeResult.Partition.Value, consumeResult.Offset.Value);
                _consumer?.Commit(consumeResult);
                return;
            }

            _logger.LogDebug(
                "Consuming message from {Topic}, EventId: {EventId}",
                _topic, @event.EventId);

            using var scope = _scopeFactory.CreateScope();
            await HandleEventAsync(scope.ServiceProvider, @event, cancellationToken);

            // 手动提交 offset
            _consumer?.Commit(consumeResult);

            _logger.LogDebug(
                "Message processed successfully from {Topic}, EventId: {EventId}",
                _topic, @event.EventId);
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Error consuming message from {Topic}", _topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {Topic}", _topic);
            // 不提交 offset，消息会被重新消费
        }
    }

    /// <summary>
    /// 处理事件的抽象方法，由具体消费者实现
    /// </summary>
    protected abstract Task HandleEventAsync(
        IServiceProvider serviceProvider,
        TEvent @event,
        CancellationToken cancellationToken);

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}
