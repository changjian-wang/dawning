using Dawning.Identity.Domain.Core.Events;

namespace Dawning.Identity.Application.Interfaces.Events;

/// <summary>
/// Domain event dispatcher interface
/// Used for publishing and handling domain events within a process (implemented via MediatR)
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatch a single domain event
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatch multiple domain events
    /// </summary>
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Integration event bus interface
/// Used for publishing events across processes/services (implemented via Kafka/RabbitMQ, etc.)
/// </summary>
public interface IIntegrationEventBus
{
    /// <summary>
    /// Publish integration event to message queue
    /// </summary>
    /// <typeparam name="TEvent">Event type</typeparam>
    /// <param name="event">Event instance</param>
    /// <param name="topic">Optional topic name (defaults to event type inference)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TEvent>(
        TEvent @event,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// Batch publish integration events
    /// </summary>
    Task PublishManyAsync<TEvent>(
        IEnumerable<TEvent> events,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent;
}

/// <summary>
/// Event handling result
/// </summary>
public enum EventHandleResult
{
    /// <summary>
    /// Processing succeeded
    /// </summary>
    Success,

    /// <summary>
    /// Processing failed, needs retry
    /// </summary>
    Retry,

    /// <summary>
    /// Processing failed, skip this message
    /// </summary>
    Skip,

    /// <summary>
    /// Processing failed, send to dead letter queue
    /// </summary>
    DeadLetter,
}
