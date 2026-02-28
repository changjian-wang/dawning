---
description: "Use when: Implementing static AutoMapper profiles, UnitOfWork service patterns, exception handling (BusinessException/NotFoundException), or defining constants (AuditConstants/CacheKeyConstants)\nDon't use when: Creating new API endpoints (use create-api), creating database tables (use create-database), reviewing code (use code-review)\nInputs: Entity/Service name or pattern to implement\nOutputs: Mapper profile, service class, exception types, or constants following project conventions\nSuccess criteria: Code follows static Mapper pattern, uses UnitOfWork correctly, exceptions have proper HTTP status codes"
---

# Code Patterns Skill

## 1. 静态 Mapper 模式

### 文件结构

```
Mapping/{Module}/
├── UserProfile.cs       # Profile + UserMappers
├── RoleProfile.cs       # Profile + RoleMappers
└── AuditLogProfile.cs   # Profile + AuditLogMappers
```

### 标准模板

```csharp
namespace YourProject.Application.Mapping;

public class {Entity}Profile : Profile
{
    public {Entity}Profile()
    {
        CreateMap<{Entity}, {Entity}Dto>();
        CreateMap<Create{Entity}Dto, {Entity}>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<Update{Entity}Dto, {Entity}>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

public static class {Entity}Mappers
{
    private static IMapper Mapper { get; }

    static {Entity}Mappers()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<{Entity}Profile>());
        Mapper = config.CreateMapper();
    }

    public static {Entity}Dto ToDto(this {Entity} entity) =>
        Mapper.Map<{Entity}Dto>(entity);

    public static IEnumerable<{Entity}Dto> ToDtos(this IEnumerable<{Entity}> entities) =>
        entities.Select(e => e.ToDto());

    public static {Entity} ToEntity(this Create{Entity}Dto dto) =>
        Mapper.Map<{Entity}>(dto);

    public static void ApplyUpdate(this {Entity} entity, Update{Entity}Dto dto) =>
        Mapper.Map(dto, entity);
}
```

### 工厂方法（审计日志等场景）

```csharp
public static class AuditLogMappers
{
    public static AuditLog CreateAuditLog(
        string action, string entityType, Guid entityId,
        string entityName, string? details = null, Guid? operatorId = null) =>
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
```

### 不适用场景

| 场景 | 原因 |
|------|------|
| 框架类型 (`DistributedCacheEntryOptions`) | 非业务实体 |
| 查询模型 (`UserModel`) | 仅用于 Repository 查询条件 |
| 配置对象 (`DestinationConfig`) | 框架配置 |

---

## 2. Service 层模式

### 标准 Service 模板

```csharp
using YourProject.Application.Mapping;
using YourProject.Domain.Constants;
using YourProject.Core.Exceptions;

public class UserService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await unitOfWork.User.GetByIdAsync(id);
        return user?.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, string? username = null)
    {
        if (await unitOfWork.User.ExistsByUsernameAsync(dto.Username))
            throw new ConflictException($"Username '{dto.Username}' already exists", "USERNAME_EXISTS");

        var user = dto.ToEntity();
        user.CreatedBy = username;
        await unitOfWork.User.InsertAsync(user);
        logger.LogInformation("User {UserId} created by {Operator}", user.Id, username);
        return user.ToDto();
    }

    public async Task<UserDto?> UpdateAsync(UpdateUserDto dto, string? username = null)
    {
        var user = await unitOfWork.User.GetByIdAsync(dto.Id);
        if (user == null) return null;

        user.ApplyUpdate(dto);
        user.UpdatedBy = username;
        await unitOfWork.User.UpdateAsync(user);
        return user.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await unitOfWork.User.DeleteAsync(id);
        return result > 0;
    }

    public async Task<PagedData<UserDto>> GetPagedListAsync(
        UserQueryModel query, int page, int pageSize)
    {
        var pagedData = await unitOfWork.User.GetPagedListAsync(query, page, pageSize);
        return new PagedData<UserDto>
        {
            Items = pagedData.Items.ToDtos(),
            TotalCount = pagedData.TotalCount,
            PageIndex = pagedData.PageIndex,
            PageSize = pagedData.PageSize
        };
    }
}
```

### Service 重构步骤

1. 创建 Mapper 类（如不存在）
2. 更新构造函数：移除 `IMapper`，移除冗余 Repository，改用 primary constructor
3. 替换映射调用：`_mapper.Map<Dto>(entity)` → `entity.ToDto()`
4. 替换 Repository 调用：`_userRepository.XXX()` → `unitOfWork.User.XXX()`
5. 使用具体异常类型，使用常量替代硬编码
6. 更新单元测试：移除 IMapper Mock，通过 UnitOfWork 设置 Repository Mock
7. 验证：`dotnet build && dotnet test`

---

## 3. 异常处理

### 异常类型体系

| 类 | HTTP 状态码 | 用途 |
|----|------------|------|
| `BusinessException` | 400 | 业务规则违反 |
| `ValidationException` | 400 | 输入验证失败 |
| `UnauthorizedException` | 401 | 未认证 |
| `ForbiddenException` | 403 | 无权限 |
| `NotFoundException` | 404 | 资源不存在 |
| `ConflictException` | 409 | 资源冲突 |

### 错误码前缀

| 前缀 | 说明 | 示例 |
|------|------|------|
| `AUTH_` | 认证相关 | `AUTH_EXPIRED`, `AUTH_INVALID_TOKEN` |
| `USER_` | 用户相关 | `USER_NOT_FOUND`, `USER_DISABLED` |
| `ROLE_` | 角色相关 | `ROLE_NOT_FOUND`, `ROLE_IN_USE` |
| `VALIDATION_` | 验证相关 | `VALIDATION_FAILED` |
| `GATEWAY_` | 网关相关 | `GATEWAY_ROUTE_NOT_FOUND` |

### 最佳实践

| 实践 | 说明 |
|------|------|
| ✅ 使用具体异常类型 | 不要直接抛出 `Exception` |
| ✅ 提供有意义的错误码 | 便于前端判断和国际化 |
| ❌ 不要暴露敏感信息 | 如堆栈信息、SQL 语句 |
| ❌ 不要吞掉异常 | 空 catch 块是反模式 |

---

## 4. 常量定义

### 目录结构

```
Domain/Constants/
├── AuditConstants.cs      # 审计操作类型 + 实体类型
├── RoleConstants.cs       # 角色名称
├── CacheKeyConstants.cs   # 缓存键（含工厂方法）
└── ConfigConstants.cs     # 配置键路径
```

### 缓存键模板

```csharp
public static class CacheKeyConstants
{
    public static class User
    {
        public static string ById(Guid id) => $"user:id:{id}";
        public static string ByUsername(string username) => $"user:username:{username}";
        public static string Permissions(Guid userId) => $"user:permissions:{userId}";
    }
}
```

### 使用规则

```csharp
// ❌ 错误 — 硬编码
await _cache.SetAsync($"user:{id}", user);

// ✅ 正确 — 使用常量
await _cache.SetAsync(CacheKeyConstants.User.ById(id), user);
```

