---
description: |
  Use when: Creating domain event records, event handler classes, audit log event handlers, and integrating events into services
  Don't use when: Creating API endpoints (use create-api), creating database tables (use create-database)
  Inputs: Event name and trigger scenario
  Outputs: Event record, EventHandler class, service integration code
  Success criteria: Event follows record pattern, handler implements IDomainEventHandler<T>, service dispatches event correctly
---

# Create Domain Event Skill

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

