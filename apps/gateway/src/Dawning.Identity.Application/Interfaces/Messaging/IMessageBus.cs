namespace Dawning.Identity.Application.Interfaces.Messaging;

/// <summary>
/// Message bus interface - Publish/Subscribe abstraction
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publish message to specified topic
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topic">Topic name</param>
    /// <param name="message">Message content</param>
    /// <param name="key">Message key (for partitioning)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(
        string topic,
        T message,
        string? key = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// Publish message (using message type name as topic)
    /// </summary>
    Task PublishAsync<T>(
        T message,
        string? key = null,
        CancellationToken cancellationToken = default
    )
        where T : class;
}

/// <summary>
/// Message handler interface
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public interface IMessageHandler<in T>
    where T : class
{
    /// <summary>
    /// Handle message
    /// </summary>
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Message consumer interface
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// Start consuming messages
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stop consuming messages
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);
}
