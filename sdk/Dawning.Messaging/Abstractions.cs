namespace Dawning.Messaging;

/// <summary>
/// Message publisher interface
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publish a message
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message content</param>
    /// <param name="routingKey">Routing key (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(
        T message,
        string? routingKey = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// Publish messages in batch
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="messages">List of messages</param>
    /// <param name="routingKey">Routing key (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishBatchAsync<T>(
        IEnumerable<T> messages,
        string? routingKey = null,
        CancellationToken cancellationToken = default
    )
        where T : class;
}

/// <summary>
/// Message subscriber interface
/// </summary>
public interface IMessageSubscriber
{
    /// <summary>
    /// Subscribe to messages
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="handler">Message handler</param>
    /// <param name="subscriptionName">Subscription name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SubscribeAsync<T>(
        Func<T, CancellationToken, Task> handler,
        string? subscriptionName = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// Unsubscribe from messages
    /// </summary>
    /// <param name="subscriptionName">Subscription name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UnsubscribeAsync(string subscriptionName, CancellationToken cancellationToken = default);
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
    /// <param name="message">Message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}
