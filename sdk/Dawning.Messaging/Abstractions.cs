namespace Dawning.Messaging;

/// <summary>
/// 消息发布者接口
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// 发布消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">消息内容</param>
    /// <param name="routingKey">路由键（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishAsync<T>(
        T message,
        string? routingKey = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// 批量发布消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="messages">消息列表</param>
    /// <param name="routingKey">路由键（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishBatchAsync<T>(
        IEnumerable<T> messages,
        string? routingKey = null,
        CancellationToken cancellationToken = default
    )
        where T : class;
}

/// <summary>
/// 消息订阅者接口
/// </summary>
public interface IMessageSubscriber
{
    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="handler">消息处理器</param>
    /// <param name="subscriptionName">订阅名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SubscribeAsync<T>(
        Func<T, CancellationToken, Task> handler,
        string? subscriptionName = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="subscriptionName">订阅名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UnsubscribeAsync(string subscriptionName, CancellationToken cancellationToken = default);
}

/// <summary>
/// 消息处理器接口
/// </summary>
/// <typeparam name="T">消息类型</typeparam>
public interface IMessageHandler<in T>
    where T : class
{
    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}
