---
description: 常量定义规范
---

# 常量定义规范

统一的常量定义和使用规范，避免硬编码字符串。

## 常量类组织

```
Domain/
├── Constants/
│   ├── AuditConstants.cs       # 审计相关常量
│   ├── RoleConstants.cs        # 角色相关常量
│   ├── CacheKeyConstants.cs    # 缓存键常量
│   └── ConfigConstants.cs      # 配置键常量
```

## 常量定义模板

### 审计常量

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
    public static class Action
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string PasswordChange = "PasswordChange";
        public const string PasswordReset = "PasswordReset";
        public const string RoleAssign = "RoleAssign";
        public const string RoleRevoke = "RoleRevoke";
        public const string PermissionGrant = "PermissionGrant";
        public const string PermissionRevoke = "PermissionRevoke";
        public const string Enable = "Enable";
        public const string Disable = "Disable";
        public const string Export = "Export";
        public const string Import = "Import";
    }

    /// <summary>
    /// 实体类型
    /// </summary>
    public static class EntityType
    {
        public const string User = "User";
        public const string Role = "Role";
        public const string Permission = "Permission";
        public const string Application = "Application";
        public const string Scope = "Scope";
        public const string ApiResource = "ApiResource";
        public const string IdentityResource = "IdentityResource";
        public const string GatewayCluster = "GatewayCluster";
        public const string GatewayRoute = "GatewayRoute";
        public const string SystemLog = "SystemLog";
        public const string AlertRule = "AlertRule";
    }
}
```

### 角色常量

```csharp
namespace YourProject.Domain.Constants;

/// <summary>
/// 系统角色常量
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// 超级管理员
    /// </summary>
    public const string SuperAdmin = "super_admin";
    
    /// <summary>
    /// 管理员
    /// </summary>
    public const string Admin = "admin";
    
    /// <summary>
    /// 用户管理员
    /// </summary>
    public const string UserManager = "user_manager";
    
    /// <summary>
    /// 审计员
    /// </summary>
    public const string Auditor = "auditor";
    
    /// <summary>
    /// 普通用户
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// 管理角色列表（用于授权策略）
    /// </summary>
    public static readonly string[] AdminRoles = [SuperAdmin, Admin];
    
    /// <summary>
    /// 用户管理角色列表
    /// </summary>
    public static readonly string[] UserManagementRoles = [SuperAdmin, Admin, UserManager, Auditor];
}
```

### 缓存键常量

```csharp
namespace YourProject.Domain.Constants;

/// <summary>
/// 缓存键常量
/// </summary>
public static class CacheKeyConstants
{
    /// <summary>
    /// 缓存键前缀
    /// </summary>
    public static class Prefix
    {
        public const string User = "user:";
        public const string Role = "role:";
        public const string Permission = "permission:";
        public const string Gateway = "gateway:";
        public const string Config = "config:";
    }

    /// <summary>
    /// 用户相关缓存键
    /// </summary>
    public static class User
    {
        public static string ById(Guid id) => $"{Prefix.User}id:{id}";
        public static string ByUsername(string username) => $"{Prefix.User}username:{username}";
        public static string Permissions(Guid userId) => $"{Prefix.User}permissions:{userId}";
        public static string Roles(Guid userId) => $"{Prefix.User}roles:{userId}";
    }

    /// <summary>
    /// 角色相关缓存键
    /// </summary>
    public static class Role
    {
        public const string All = $"{Prefix.Role}all";
        public static string ById(Guid id) => $"{Prefix.Role}id:{id}";
        public static string ByName(string name) => $"{Prefix.Role}name:{name}";
    }

    /// <summary>
    /// 网关配置缓存键
    /// </summary>
    public static class Gateway
    {
        public const string Routes = $"{Prefix.Gateway}routes";
        public const string Clusters = $"{Prefix.Gateway}clusters";
        public static string Route(string routeId) => $"{Prefix.Gateway}route:{routeId}";
    }
}
```

### 配置键常量

```csharp
namespace YourProject.Domain.Constants;

/// <summary>
/// 配置键常量
/// </summary>
public static class ConfigConstants
{
    /// <summary>
    /// JWT 配置
    /// </summary>
    public static class Jwt
    {
        public const string Section = "Jwt";
        public const string Secret = "Jwt:Secret";
        public const string Issuer = "Jwt:Issuer";
        public const string Audience = "Jwt:Audience";
        public const string ExpirationMinutes = "Jwt:ExpirationMinutes";
    }

    /// <summary>
    /// 数据库配置
    /// </summary>
    public static class Database
    {
        public const string ConnectionString = "ConnectionStrings:DefaultConnection";
    }

    /// <summary>
    /// Redis 配置
    /// </summary>
    public static class Redis
    {
        public const string ConnectionString = "Redis:ConnectionString";
        public const string InstanceName = "Redis:InstanceName";
    }
}
```

## 使用示例

### ❌ 错误 - 硬编码字符串

```csharp
// 审计日志中硬编码
await _unitOfWork.AuditLog.InsertAsync(new AuditLog
{
    Action = "Create",           // ❌ 硬编码
    EntityType = "User",         // ❌ 硬编码
    EntityId = user.Id.ToString()
});

// 角色检查中硬编码
if (user.Roles.Contains("admin"))  // ❌ 硬编码
{
    // ...
}

// 缓存键硬编码
await _cache.SetAsync($"user:{id}", user);  // ❌ 硬编码
```

### ✅ 正确 - 使用常量

```csharp
// 使用常量类
await _unitOfWork.AuditLog.InsertAsync(new AuditLog
{
    Action = AuditConstants.Action.Create,
    EntityType = AuditConstants.EntityType.User,
    EntityId = user.Id.ToString()
});

// 角色检查使用常量
if (user.Roles.Contains(RoleConstants.Admin))
{
    // ...
}

// 缓存键使用常量方法
await _cache.SetAsync(CacheKeyConstants.User.ById(id), user);
```

### Mapper 工厂方法中使用常量

```csharp
public static class AuditLogMappers
{
    public static AuditLog CreateAuditLog(
        string action,
        string entityType,
        Guid entityId,
        string entityName,
        string? details = null,
        Guid? operatorId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            EntityName = entityName,
            Details = details,
            OperatorId = operatorId,
            CreatedAt = DateTime.UtcNow
        };
}

// 使用
var log = AuditLogMappers.CreateAuditLog(
    AuditConstants.Action.Create,
    AuditConstants.EntityType.User,
    user.Id,
    user.Username,
    "用户创建成功"
);
```

## 命名规范

| 类型 | 命名规范 | 示例 |
|------|----------|------|
| 常量类 | `{Domain}Constants` | `AuditConstants`, `RoleConstants` |
| 嵌套类 | PascalCase | `Action`, `EntityType`, `Prefix` |
| 常量值 | PascalCase | `Create`, `SuperAdmin` |
| 方法 | PascalCase | `ById(Guid id)` |

## 最佳实践

| 实践 | 说明 |
|------|------|
| ✅ 集中定义 | 常量放在 `Constants/` 目录 |
| ✅ 分类组织 | 使用嵌套类分组相关常量 |
| ✅ 添加注释 | 每个常量添加 XML 注释 |
| ✅ 使用 const | 字符串常量使用 `const` |
| ✅ 使用 static readonly | 数组常量使用 `static readonly` |
| ❌ 避免魔法字符串 | 不在代码中直接写字符串字面量 |
