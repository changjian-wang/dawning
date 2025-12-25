# DDD Event-Driven Architecture

## 概述

本项目采用 **领域驱动设计 (DDD)** 的事件驱动架构，实现了进程内和进程间的事件通信机制：

- **MediatR**: 进程内领域事件 (同步)
- **Kafka**: 进程间集成事件 (异步)

## 架构分层

```
┌─────────────────────────────────────────────────────────────────┐
│                     Presentation Layer                          │
│                   (Dawning.Identity.Api)                        │
│                                                                 │
│  Controllers → Services → Events → Responses                    │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                    Application Layer                             │
│                (Dawning.Identity.Application)                    │
│                                                                  │
│  ┌─────────────────┐  ┌─────────────────────┐                   │
│  │ Interfaces      │  │ Event Handlers      │                   │
│  │ - IEventBus.cs  │  │ (MediatR Handlers)  │                   │
│  │   - IDomainEvent│  │ UserCreatedHandler  │                   │
│  │     Dispatcher  │  │ UserLoggedInHandler │                   │
│  │   - IIntegration│  │ RoleAssignedHandler │                   │
│  │     EventBus    │  │ ...                 │                   │
│  └─────────────────┘  └─────────────────────┘                   │
│                                                                  │
│  ┌─────────────────────────────────────────┐                    │
│  │ Integration Events (DTOs)               │                    │
│  │ - AuditLogIntegrationEvent              │                    │
│  │ - AlertNotificationIntegrationEvent     │                    │
│  │ - EmailIntegrationEvent                 │                    │
│  │ - CacheInvalidationIntegrationEvent     │                    │
│  │ - UserEventIntegrationEvent             │                    │
│  │ - SystemEventIntegrationEvent           │                    │
│  └─────────────────────────────────────────┘                    │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                      Domain Layer                                │
│                  (Dawning.Identity.Domain)                       │
│                                                                  │
│  ┌─────────────────────────────────────────┐                    │
│  │ Domain Events (Concrete)                │                    │
│  │ - UserCreatedEvent                      │                    │
│  │ - UserLoggedInEvent                     │                    │
│  │ - UserPasswordChangedEvent              │                    │
│  │ - RoleAssignedEvent                     │                    │
│  │ - ConfigurationChangedEvent             │                    │
│  │ - AlertTriggeredEvent                   │                    │
│  │ - EntityChangedEvent<T>                 │                    │
│  └─────────────────────────────────────────┘                    │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                   Domain.Core Layer                              │
│               (Dawning.Identity.Domain.Core)                     │
│                                                                  │
│  ┌─────────────────┐  ┌─────────────────────┐                   │
│  │ Events          │  │ Interfaces          │                   │
│  │ - IDomainEvent  │  │ - IHasDomainEvents  │                   │
│  │   : INotification│ │ - Entity (base)     │                   │
│  │ - DomainEvent   │  │                     │                   │
│  │ - IIntegration  │  │                     │                   │
│  │   Event         │  │                     │                   │
│  │ - IntegrationEvt│  │                     │                   │
│  └─────────────────┘  └─────────────────────┘                   │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                  Infrastructure Layer                            │
│              (Dawning.Identity.Infra.Messaging)                  │
│                                                                  │
│  ┌─────────────────────────────────────────┐                    │
│  │ Implementations                         │                    │
│  │                                         │                    │
│  │ MediatRDomainEventDispatcher            │                    │
│  │   → 进程内领域事件分发                    │                    │
│  │                                         │                    │
│  │ KafkaIntegrationEventBus                │                    │
│  │   → 进程间集成事件发布 (Producer)         │                    │
│  │                                         │                    │
│  │ KafkaIntegrationEventConsumer           │                    │
│  │   → 集成事件消费基类                      │                    │
│  │                                         │                    │
│  │ IntegrationEventConsumers               │                    │
│  │   → 具体消费者实现                        │                    │
│  │                                         │                    │
│  │ RedisDistributedLock                    │                    │
│  │   → 分布式锁实现                          │                    │
│  └─────────────────────────────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
```

## 事件流程

### 1. 领域事件 (MediatR - 进程内同步)

```
User Action → Controller → Application Service
                               ↓
                    Entity.AddDomainEvent(event)
                               ↓
                    IDomainEventDispatcher.DispatchEventsAsync()
                               ↓
                    MediatR.Publish(INotification)
                               ↓
                    INotificationHandler<TEvent>.Handle()
                               ↓
                    IIntegrationEventBus.PublishAsync()
```

**示例**: 用户创建流程
```csharp
// 1. 领域实体添加事件
user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email, user.Name));

// 2. Service 分发事件
await _domainEventDispatcher.DispatchEventsAsync(user);

// 3. MediatR Handler 处理
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 发布集成事件到 Kafka
        await _integrationEventBus.PublishAsync(new UserEventIntegrationEvent
        {
            EventType = "UserCreated",
            UserId = notification.UserId,
            Email = notification.Email
        });
    }
}
```

### 2. 集成事件 (Kafka - 进程间异步)

```
Kafka Producer (IIntegrationEventBus)
           ↓
    [dawning.user-event Topic]
           ↓
Kafka Consumer (BackgroundService)
           ↓
    Process Event in Target Service
```

**Kafka Topics**:
- `dawning.audit-log` - 审计日志
- `dawning.alert-notification` - 告警通知
- `dawning.email` - 邮件发送
- `dawning.config-changed` - 配置变更
- `dawning.cache-invalidation` - 缓存失效
- `dawning.user-event` - 用户事件

## 核心接口

### IDomainEvent (Domain.Core)
```csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
```

### IIntegrationEvent (Domain.Core)
```csharp
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime CreatedAt { get; }
    string EventType { get; }
}
```

### IDomainEventDispatcher (Application)
```csharp
public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IHasDomainEvents entity, CancellationToken cancellationToken = default);
    Task DispatchEventsAsync(IEnumerable<IHasDomainEvents> entities, CancellationToken cancellationToken = default);
}
```

### IIntegrationEventBus (Application)
```csharp
public interface IIntegrationEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent;
    Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent;
}
```

## 配置

### appsettings.json
```json
{
  "Kafka": {
    "Enabled": true,
    "BootstrapServers": "localhost:9092",
    "Topics": {
      "AuditLog": "dawning.audit-log",
      "AlertNotification": "dawning.alert-notification",
      "Email": "dawning.email",
      "ConfigChanged": "dawning.config-changed",
      "CacheInvalidation": "dawning.cache-invalidation",
      "UserEvent": "dawning.user-event"
    },
    "Producer": {
      "Acks": "All",
      "EnableIdempotence": true,
      "Retries": 3,
      "LingerMs": 5,
      "BatchSize": 16384
    },
    "Consumer": {
      "GroupId": "dawning-identity-consumer",
      "AutoOffsetReset": "Earliest",
      "EnableAutoCommit": false
    }
  }
}
```

### 服务注册 (Program.cs)
```csharp
// 添加 MediatR + Kafka 消息服务
builder.Services.AddMessagingServices(configuration);
```

## 分布式支持

### 分布式锁 (Redis)
```csharp
public interface IDistributedLock
{
    Task<IDistributedLockHandle> AcquireAsync(string lockKey, TimeSpan expiry, CancellationToken cancellationToken = default);
}

// 使用示例
await using var lockHandle = await _distributedLock.AcquireAsync("my-lock", TimeSpan.FromSeconds(30));
if (lockHandle.IsAcquired)
{
    // 执行互斥操作
}
```

### Kafka 消费者组
- 多个实例可以加入同一消费者组
- Kafka 自动分配分区，实现负载均衡
- 每条消息只被组内一个消费者处理

## 扩展指南

### 添加新的领域事件

1. 在 `Domain/Events/` 创建事件类：
```csharp
public sealed class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public OrderCreatedEvent(Guid orderId) => OrderId = orderId;
}
```

2. 在 `Application/EventHandlers/` 创建处理器：
```csharp
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 处理逻辑
    }
}
```

### 添加新的集成事件消费者

1. 在 `Infra.Messaging/Kafka/Consumers/` 创建消费者：
```csharp
public class OrderEventConsumer : KafkaIntegrationEventConsumer<OrderIntegrationEvent>
{
    protected override string Topic => "dawning.order-event";
    protected override string GroupId => "dawning-order-consumer";
    
    protected override Task ProcessEventAsync(OrderIntegrationEvent @event, CancellationToken cancellationToken)
    {
        // 处理逻辑
    }
}
```

2. 在 `MessagingServiceExtensions` 中注册：
```csharp
services.AddHostedService<OrderEventConsumer>();
```

## 最佳实践

1. **事件命名**: 使用过去式 (UserCreated, OrderPlaced)
2. **事件不可变**: 所有属性只读
3. **幂等处理**: 消费者应支持重复消费
4. **错误处理**: 使用死信队列处理失败消息
5. **版本控制**: 考虑事件版本化以支持演进

## 相关文件

- [Dawning.Identity.Domain.Core/Events/IDomainEvent.cs](../src/Dawning.Identity.Domain.Core/Events/IDomainEvent.cs)
- [Dawning.Identity.Domain/Events/DomainEvents.cs](../src/Dawning.Identity.Domain/Events/DomainEvents.cs)
- [Dawning.Identity.Application/Interfaces/Events/IEventBus.cs](../src/Dawning.Identity.Application/Interfaces/Events/IEventBus.cs)
- [Dawning.Identity.Application/EventHandlers/DomainEventHandlers.cs](../src/Dawning.Identity.Application/EventHandlers/DomainEventHandlers.cs)
- [Dawning.Identity.Infra.Messaging/](../src/Dawning.Identity.Infra.Messaging/)
