using MediatR;

namespace Dawning.Identity.Domain.Core.Events;

/// <summary>
/// Domain event marker interface
/// All domain events should implement this interface
/// Inherits INotification to support MediatR publish/subscribe
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Event unique identifier
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Event occurrence time
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Event type name
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Domain event base class
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public virtual string EventType => GetType().Name;
}

/// <summary>
/// Integration event marker interface
/// Used for cross-service/process events (published via message queue)
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Event unique identifier
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Event occurrence time
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Correlation ID (for distributed tracing)
    /// </summary>
    string? CorrelationId { get; set; }
}

/// <summary>
/// Integration event base class
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}

