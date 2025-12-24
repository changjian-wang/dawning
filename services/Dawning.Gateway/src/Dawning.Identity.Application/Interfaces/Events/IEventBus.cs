using Dawning.Identity.Domain.Core.Events;

namespace Dawning.Identity.Application.Interfaces.Events;

/// <summary>
/// 领域事件分发器接口
/// 用于在进程内发布和处理领域事件（通过 MediatR 实现）
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 发布单个领域事件
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布多个领域事件
    /// </summary>
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// 集成事件总线接口
/// 用于跨进程/服务发布事件（通过 Kafka/RabbitMQ 等实现）
/// </summary>
public interface IIntegrationEventBus
{
    /// <summary>
    /// 发布集成事件到消息队列
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">事件实例</param>
    /// <param name="topic">可选的主题名称（默认根据事件类型推断）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishAsync<TEvent>(
        TEvent @event,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// 批量发布集成事件
    /// </summary>
    Task PublishManyAsync<TEvent>(
        IEnumerable<TEvent> events,
        string? topic = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent;
}

/// <summary>
/// 事件处理结果
/// </summary>
public enum EventHandleResult
{
    /// <summary>
    /// 处理成功
    /// </summary>
    Success,

    /// <summary>
    /// 处理失败，需要重试
    /// </summary>
    Retry,

    /// <summary>
    /// 处理失败，跳过此消息
    /// </summary>
    Skip,

    /// <summary>
    /// 处理失败，发送到死信队列
    /// </summary>
    DeadLetter,
}
