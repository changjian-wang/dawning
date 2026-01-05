---
description: 创建领域事件和事件处理器
---

# 创建领域事件

创建领域事件（Domain Event）和对应的事件处理器。

## 领域事件概述

领域事件用于解耦核心业务逻辑与副作用操作（如审计日志、通知、缓存刷新等）。

## 文件结构

```
Domain/
├── Events/
│   ├── UserCreatedEvent.cs
│   ├── UserUpdatedEvent.cs
│   └── UserDeletedEvent.cs
Application/
├── EventHandlers/
│   ├── UserEventHandlers.cs
│   └── AuditLogEventHandler.cs
```

## 创建流程

### 1. 定义事件类

```csharp
namespace YourProject.Domain.Events;

/// <summary>
/// 用户创建事件
/// </summary>
public record UserCreatedEvent(
    Guid UserId,
    string Username,
    string? Email,
    Guid? OperatorId,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// 用户更新事件
/// </summary>
public record UserUpdatedEvent(
    Guid UserId,
    string Username,
    Dictionary<string, (object? OldValue, object? NewValue)>? Changes,
    Guid? OperatorId,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// 用户删除事件
/// </summary>
public record UserDeletedEvent(
    Guid UserId,
    string Username,
    Guid? OperatorId,
    DateTime OccurredAt
) : IDomainEvent;
```

### 2. 创建事件处理器

```csharp
namespace YourProject.Application.EventHandlers;

using YourProject.Domain.Events;
using YourProject.Domain.Interfaces.UoW;

/// <summary>
/// 用户事件处理器 - 处理审计日志
/// </summary>
public class UserAuditEventHandler(IUnitOfWork unitOfWork)
{
    /// <summary>
    /// 处理用户创建事件
    /// </summary>
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

    /// <summary>
    /// 处理用户更新事件
    /// </summary>
    public async Task HandleAsync(UserUpdatedEvent @event)
    {
        var details = @event.Changes != null 
            ? string.Join(", ", @event.Changes.Select(c => $"{c.Key}: {c.Value.OldValue} -> {c.Value.NewValue}"))
            : "用户信息已更新";
            
        var auditLog = AuditLogMappers.CreateAuditLog(
            AuditConstants.AuditAction.Update,
            AuditConstants.AuditEntityType.User,
            @event.UserId,
            @event.Username,
            details,
            @event.OperatorId
        );
        
        await unitOfWork.AuditLog.InsertAsync(auditLog);
    }

    /// <summary>
    /// 处理用户删除事件
    /// </summary>
    public async Task HandleAsync(UserDeletedEvent @event)
    {
        var auditLog = AuditLogMappers.CreateAuditLog(
            AuditConstants.AuditAction.Delete,
            AuditConstants.AuditEntityType.User,
            @event.UserId,
            @event.Username,
            $"用户 {@event.Username} 已删除",
            @event.OperatorId
        );
        
        await unitOfWork.AuditLog.InsertAsync(auditLog);
    }
}
```

### 3. 在 Service 中发布事件

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
        
        // 发布领域事件
        await eventDispatcher.PublishAsync(new UserCreatedEvent(
            user.Id,
            user.Username,
            user.Email,
            operatorId,
            DateTime.UtcNow
        ));
        
        return user.ToDto();
    }
}
```

## 常量定义

使用常量类避免硬编码字符串：

```csharp
namespace YourProject.Domain.Constants;

/// <summary>
/// 审计日志常量
/// </summary>
public static class AuditConstants
{
    /// <summary>
    /// 审计操作类型
    /// </summary>
    public static class AuditAction
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string PasswordChange = "PasswordChange";
        public const string RoleAssign = "RoleAssign";
        public const string PermissionGrant = "PermissionGrant";
    }

    /// <summary>
    /// 实体类型
    /// </summary>
    public static class AuditEntityType
    {
        public const string User = "User";
        public const string Role = "Role";
        public const string Permission = "Permission";
        public const string Application = "Application";
        public const string Scope = "Scope";
        public const string GatewayCluster = "GatewayCluster";
        public const string GatewayRoute = "GatewayRoute";
    }
}
```

## 最佳实践

| 实践 | 说明 |
|------|------|
| 使用 record | 事件类使用 record 类型，不可变 |
| 包含时间戳 | 每个事件包含 `OccurredAt` 时间戳 |
| 包含操作者 | 包含 `OperatorId` 追踪操作人 |
| 使用常量 | 使用常量类避免硬编码字符串 |
| 异步处理 | 事件处理器使用异步方法 |
| 解耦处理 | 一个事件可以有多个处理器 |

## 常见事件类型

| 事件类型 | 触发时机 | 处理内容 |
|----------|----------|----------|
| `EntityCreatedEvent` | 实体创建后 | 审计日志、发送通知 |
| `EntityUpdatedEvent` | 实体更新后 | 审计日志、缓存刷新 |
| `EntityDeletedEvent` | 实体删除后 | 审计日志、清理关联 |
| `UserLoginEvent` | 用户登录后 | 登录日志、更新最后登录时间 |
| `PasswordChangedEvent` | 密码修改后 | 审计日志、发送安全通知 |
