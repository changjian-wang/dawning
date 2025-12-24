using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Domain.Core.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Infra.Messaging;

/// <summary>
/// 基于 MediatR 的领域事件分发器
/// 用于进程内发布领域事件
/// </summary>
public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatRDomainEventDispatcher> _logger;

    public MediatRDomainEventDispatcher(
        IMediator mediator,
        ILogger<MediatRDomainEventDispatcher> logger
    )
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 发布单个领域事件
    /// </summary>
    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug(
            "Dispatching domain event: {EventType}, EventId: {EventId}",
            domainEvent.EventType,
            domainEvent.EventId
        );

        try
        {
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogDebug(
                "Domain event dispatched successfully: {EventType}",
                domainEvent.EventType
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error dispatching domain event: {EventType}, EventId: {EventId}",
                domainEvent.EventType,
                domainEvent.EventId
            );
            throw;
        }
    }

    /// <summary>
    /// 发布多个领域事件（按顺序）
    /// </summary>
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
