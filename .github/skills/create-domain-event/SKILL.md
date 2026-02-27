---
description: "Create domain events and event handlers for Dawning: event definition, audit log handler, service integration. Trigger: 领域事件, domain event, 事件处理, event handler, 事件发布, publish event"
---

# Create Domain Event Skill

## 目标

创建领域事件和事件处理器，解耦核心业务逻辑与副作用操作。

## 触发条件

- **关键词**：领域事件, domain event, 事件处理, event handler, 事件发布, publish event, 解耦
- **文件模式**：`*Event.cs`, `*EventHandler.cs`, `Domain/Events/**`
- **用户意图**：创建领域事件、实现事件驱动

## 编排

- **前置**：`create-api`（Service 层就绪）
- **后续**：`create-tests`（事件处理器需要测试）

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} create-domain-event — {触发原因}`

---

## 文件结构

```
Domain/Events/
├── UserCreatedEvent.cs
├── UserUpdatedEvent.cs
└── UserDeletedEvent.cs
Application/EventHandlers/
├── UserEventHandlers.cs
└── AuditLogEventHandler.cs
```

## 事件定义

```csharp
namespace YourProject.Domain.Events;

public record UserCreatedEvent(
    Guid UserId,
    string Username,
    string? Email,
    Guid? OperatorId,
    DateTime OccurredAt
) : IDomainEvent;

public record UserUpdatedEvent(
    Guid UserId,
    string Username,
    Dictionary<string, (object? OldValue, object? NewValue)>? Changes,
    Guid? OperatorId,
    DateTime OccurredAt
) : IDomainEvent;

public record UserDeletedEvent(
    Guid UserId,
    string Username,
    Guid? OperatorId,
    DateTime OccurredAt
) : IDomainEvent;
```

## 事件处理器

```csharp
public class UserAuditEventHandler(IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(UserCreatedEvent @event)
    {
        var auditLog = AuditLogMappers.CreateAuditLog(
            AuditConstants.AuditAction.Create,
            AuditConstants.AuditEntityType.User,
            @event.UserId,
            @event.Username,
            $"用户 {@event.Username} 创建成功",
            @event.OperatorId
        );
        await unitOfWork.AuditLog.InsertAsync(auditLog);
    }
}
```

## Service 集成

```csharp
public class UserService(
    IUnitOfWork unitOfWork,
    IDomainEventDispatcher eventDispatcher
) : IUserService
{
    public async Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId)
    {
        var user = dto.ToEntity();
        user.CreatedBy = operatorId?.ToString();
        await unitOfWork.User.InsertAsync(user);

        await eventDispatcher.PublishAsync(new UserCreatedEvent(
            user.Id, user.Username, user.Email, operatorId, DateTime.UtcNow
        ));

        return user.ToDto();
    }
}
```

## 验收场景

- **输入**："创建 Order 的领域事件"
- **预期**：agent 生成 OrderCreatedEvent/UpdatedEvent/DeletedEvent + 审计日志处理器
- **上次验证**：2026-02-27 ✅
