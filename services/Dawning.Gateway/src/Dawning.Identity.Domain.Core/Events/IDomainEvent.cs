using MediatR;

namespace Dawning.Identity.Domain.Core.Events;

/// <summary>
/// 领域事件标记接口
/// 所有领域事件都应实现此接口
/// 继承 INotification 以支持 MediatR 发布/订阅
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// 事件唯一标识
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// 事件发生时间
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// 事件类型名称
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// 领域事件基类
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public virtual string EventType => GetType().Name;
}

/// <summary>
/// 集成事件标记接口
/// 用于跨服务/进程的事件（通过消息队列发布）
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// 事件唯一标识
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// 事件发生时间
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// 关联 ID（用于分布式追踪）
    /// </summary>
    string? CorrelationId { get; set; }
}

/// <summary>
/// 集成事件基类
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}
