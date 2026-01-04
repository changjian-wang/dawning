using System.Text.Json;
using Confluent.Kafka;
using Dawning.Identity.Application.Dtos.IntegrationEvents;
using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Application.Interfaces.Messaging;
using Dawning.Identity.Domain.Core.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Identity.Infra.Messaging.Kafka;

/// <summary>
/// Kafka-based integration event bus implementation
/// Used for cross-process/service event publishing
/// </summary>
public class KafkaIntegrationEventBus : IIntegrationEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaIntegrationEventBus> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public KafkaIntegrationEventBus(
        IOptions<KafkaOptions> options,
        ILogger<KafkaIntegrationEventBus> logger
    )
    {
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Enum.Parse<Acks>(_options.Producer.Acks, true),
            EnableIdempotence = _options.Producer.EnableIdempotence,
            MaxInFlight = _options.Producer.MaxInFlight,
            LingerMs = _options.Producer.LingerMs,
            BatchSize = _options.Producer.BatchSize,
            CompressionType = Enum.Parse<CompressionType>(_options.Producer.CompressionType, true),
            MessageTimeoutMs = 30000,
            RequestTimeoutMs = 30000,
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler(
                (_, e) =>
                {
                    _logger.LogError("Kafka producer error: {Reason}", e.Reason);
                }
            )
            .Build();

        _logger.LogInformation(
            "Kafka integration event bus initialized. BootstrapServers: {Servers}",
            _options.BootstrapServers
        );
    }

    /// <summary>
    /// Publish integration event to Kafka
    /// </summary>
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent
    {
        var topicName = topic ?? GetTopicForEvent<TEvent>();
        var messageKey = @event.EventId.ToString();
        var messageValue = JsonSerializer.Serialize(@event, _jsonOptions);

        try
        {
            var result = await _producer.ProduceAsync(
                topicName,
                new Message<string, string>
                {
                    Key = messageKey,
                    Value = messageValue,
                    Headers = new Headers
                    {
                        { "event-type", System.Text.Encoding.UTF8.GetBytes(typeof(TEvent).Name) },
                        {
                            "correlation-id",
                            System.Text.Encoding.UTF8.GetBytes(@event.CorrelationId ?? "")
                        },
                        {
                            "occurred-on",
                            System.Text.Encoding.UTF8.GetBytes(@event.OccurredOn.ToString("O"))
                        },
                    },
                },
                cancellationToken
            );

            _logger.LogDebug(
                "Integration event published - Topic: {Topic}, EventId: {EventId}, Partition: {Partition}, Offset: {Offset}",
                topicName,
                @event.EventId,
                result.Partition.Value,
                result.Offset.Value
            );
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish integration event - Topic: {Topic}, EventId: {EventId}",
                topicName,
                @event.EventId
            );
            throw;
        }
    }

    /// <summary>
    /// Batch publish integration events
    /// </summary>
    public async Task PublishManyAsync<TEvent>(
        IEnumerable<TEvent> events,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, topic, cancellationToken);
        }

        // Ensure all messages are sent
        _producer.Flush(cancellationToken);
    }

    /// <summary>
    /// Get Topic name based on event type
    /// </summary>
    private string GetTopicForEvent<TEvent>()
        where TEvent : IIntegrationEvent
    {
        var eventTypeName = typeof(TEvent).Name;

        return eventTypeName switch
        {
            nameof(AuditLogIntegrationEvent) => _options.Topics.AuditLog,
            nameof(AlertNotificationIntegrationEvent) => _options.Topics.AlertNotification,
            nameof(EmailIntegrationEvent) => _options.Topics.Email,
            nameof(ConfigChangedIntegrationEvent) => _options.Topics.ConfigChanged,
            nameof(CacheInvalidationIntegrationEvent) => _options.Topics.CacheInvalidation,
            nameof(UserEventIntegrationEvent) => _options.Topics.UserEvent,
            nameof(SystemEventIntegrationEvent) => _options.Topics.SystemEvent,
            _ => $"dawning.{ToKebabCase(eventTypeName.Replace("IntegrationEvent", ""))}",
        };
    }

    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                if (i > 0)
                    result.Append('-');
                result.Append(char.ToLower(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }
        return result.ToString();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Null implementation of integration event bus (used when Kafka is disabled)
/// </summary>
public class NullIntegrationEventBus : IIntegrationEventBus
{
    private readonly ILogger<NullIntegrationEventBus> _logger;

    public NullIntegrationEventBus(ILogger<NullIntegrationEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(
        TEvent @event,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent
    {
        _logger.LogDebug(
            "NullIntegrationEventBus: Event {EventType} would be published to {Topic}",
            typeof(TEvent).Name,
            topic ?? "default"
        );
        return Task.CompletedTask;
    }

    public Task PublishManyAsync<TEvent>(
        IEnumerable<TEvent> events,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent
    {
        _logger.LogDebug(
            "NullIntegrationEventBus: {Count} events would be published to {Topic}",
            events.Count(),
            topic ?? "default"
        );
        return Task.CompletedTask;
    }
}
