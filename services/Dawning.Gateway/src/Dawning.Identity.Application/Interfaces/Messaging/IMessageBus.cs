namespace Dawning.Identity.Application.Interfaces.Messaging;

/// <summary>
/// 消息总线接口 - 发布/订阅抽象
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// 发布消息到指定主题
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="topic">主题名称</param>
    /// <param name="message">消息内容</param>
    /// <param name="key">消息键（用于分区）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 发布消息（使用消息类型名作为主题）
    /// </summary>
    Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// 消息处理器接口
/// </summary>
/// <typeparam name="T">消息类型</typeparam>
public interface IMessageHandler<in T> where T : class
{
    /// <summary>
    /// 处理消息
    /// </summary>
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}

/// <summary>
/// 消息消费者接口
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// 开始消费消息
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 停止消费消息
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);
}
