using Dawning.Identity.Domain.Core.Events;

namespace Dawning.Identity.Domain.Core.Interfaces;

/// <summary>
/// 可发布领域事件的聚合根接口
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// 获取待发布的领域事件
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 添加领域事件
    /// </summary>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// 移除领域事件
    /// </summary>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// 清空所有领域事件
    /// </summary>
    void ClearDomainEvents();
}

/// <summary>
/// 支持领域事件的实体基类
/// </summary>
public abstract class Entity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// 触发领域事件
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
