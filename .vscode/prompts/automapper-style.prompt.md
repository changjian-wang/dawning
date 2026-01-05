---
description: AutoMapper 静态映射器代码风格规范
---

# AutoMapper 静态映射器风格指南

本项目使用 **静态 Mapper 扩展方法** 模式，而非传统的 `IMapper` 实例注入。这种风格更简洁、性能更好、且易于测试。

## 🎯 核心原则

1. **Profile + Mappers 共存**：每个 Profile 文件同时包含 AutoMapper 配置和静态映射器类
2. **扩展方法风格**：使用 `entity.ToDto()` 而非 `_mapper.Map<Dto>(entity)`
3. **无依赖注入**：Service 不需要注入 `IMapper`，直接调用扩展方法
4. **静态初始化**：Mapper 在静态构造函数中初始化一次

## 📁 文件结构

```
Mapping/
└── Administration/
    ├── UserProfile.cs          # Profile + UserMappers
    ├── RoleProfile.cs          # Profile + RoleMappers
    ├── PermissionProfile.cs    # Profile + PermissionMappers
    └── AuditLogProfile.cs      # Profile + AuditLogMappers
```

## ✅ 标准模板

### Profile + Mappers 文件模板

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using YourProject.Application.Dtos;
using YourProject.Domain.Aggregates;

namespace YourProject.Application.Mapping;

/// <summary>
/// {Entity} AutoMapper 配置
/// </summary>
public class {Entity}Profile : Profile
{
    public {Entity}Profile()
    {
        // Domain -> DTO
        CreateMap<{Entity}, {Entity}Dto>();
        
        // CreateDto -> Domain
        CreateMap<Create{Entity}Dto, {Entity}>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
            
        // UpdateDto -> Domain (条件映射，跳过 null)
        CreateMap<Update{Entity}Dto, {Entity}>()
            .ForAllMembers(opts => 
                opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

/// <summary>
/// {Entity} 静态映射器
/// </summary>
public static class {Entity}Mappers
{
    private static IMapper Mapper { get; }

    static {Entity}Mappers()
    {
        var config = new MapperConfiguration(cfg => 
            cfg.AddProfile<{Entity}Profile>());
        Mapper = config.CreateMapper();
    }

    /// <summary>
    /// 转换为 DTO
    /// </summary>
    public static {Entity}Dto ToDto(this {Entity} entity) =>
        Mapper.Map<{Entity}Dto>(entity);

    /// <summary>
    /// 批量转换为 DTO
    /// </summary>
    public static IEnumerable<{Entity}Dto> ToDtos(this IEnumerable<{Entity}> entities) =>
        entities.Select(e => e.ToDto());

    /// <summary>
    /// CreateDto 转换为实体
    /// </summary>
    public static {Entity} ToEntity(this Create{Entity}Dto dto) =>
        Mapper.Map<{Entity}>(dto);

    /// <summary>
    /// 应用更新（将 UpdateDto 的非空值应用到实体）
    /// </summary>
    public static void ApplyUpdate(this {Entity} entity, Update{Entity}Dto dto) =>
        Mapper.Map(dto, entity);
}
```

## 📝 Service 层使用示例

### ❌ 旧风格（实例注入）

```csharp
public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;  // ❌ 需要注入

    public UserService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetAsync(Guid id)
    {
        var user = await _uow.User.GetAsync(id);
        return user != null ? _mapper.Map<UserDto>(user) : null;  // ❌ 实例调用
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId)
    {
        var user = _mapper.Map<User>(dto);  // ❌ 实例调用
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.CreatedBy = operatorId;
        
        await _uow.User.InsertAsync(user);
        return _mapper.Map<UserDto>(user);  // ❌ 实例调用
    }
}
```

### ✅ 新风格（静态扩展方法）

```csharp
using YourProject.Application.Mapping;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    // ✅ 无需注入 IMapper

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetAsync(Guid id)
    {
        var user = await _unitOfWork.User.GetAsync(id);
        return user?.ToDto();  // ✅ 扩展方法，简洁
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.User.GetAllAsync();
        return users.ToDtos();  // ✅ 批量转换
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId)
    {
        var user = dto.ToEntity();  // ✅ 扩展方法
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.CreatedBy = operatorId;
        
        await _unitOfWork.User.InsertAsync(user);
        return user.ToDto();  // ✅ 扩展方法
    }

    public async Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid? operatorId)
    {
        var user = await _unitOfWork.User.GetAsync(dto.Id);
        if (user == null) throw new NotFoundException("User not found");
        
        user.ApplyUpdate(dto);  // ✅ 应用更新
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = operatorId;
        
        await _unitOfWork.User.UpdateAsync(user);
        return user.ToDto();
    }
}
```

## 🏭 工厂方法模式（用于审计日志等场景）

当需要创建带有预设值的实体时，使用工厂方法：

```csharp
public static class AuditLogMappers
{
    private static IMapper Mapper { get; }
    
    static AuditLogMappers() { /* ... */ }

    // 标准映射方法
    public static AuditLogDto ToDto(this AuditLog log) => Mapper.Map<AuditLogDto>(log);
    public static AuditLog ToEntity(this CreateAuditLogDto dto) => Mapper.Map<AuditLog>(dto);

    // 🏭 工厂方法 - 快速创建特定类型的审计日志
    public static AuditLog CreateAudit(string entityType, Guid entityId, string details, Guid? operatorId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Action = AuditAction.Create,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            Details = details,
            OperatorId = operatorId,
            CreatedAt = DateTime.UtcNow
        };

    public static AuditLog UpdateAudit(string entityType, Guid entityId, string details, Guid? operatorId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Action = AuditAction.Update,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            Details = details,
            OperatorId = operatorId,
            CreatedAt = DateTime.UtcNow
        };

    public static AuditLog DeleteAudit(string entityType, Guid entityId, string details, Guid? operatorId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Action = AuditAction.Delete,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            Details = details,
            OperatorId = operatorId,
            CreatedAt = DateTime.UtcNow
        };
}
```

## 📊 分页数据转换

```csharp
public async Task<PagedData<UserDto>> GetPagedListAsync(UserModel model, int page, int pageSize)
{
    var pagedData = await _uow.User.GetPagedListAsync(model, page, pageSize);

    return new PagedData<UserDto>
    {
        PageIndex = pagedData.PageIndex,
        PageSize = pagedData.PageSize,
        TotalCount = pagedData.TotalCount,
        Items = pagedData.Items.ToDtos()  // ✅ 使用扩展方法
    };
}
```

## ⚠️ 不适用场景

以下情况**不需要**使用静态 Mappers：

| 场景 | 示例 | 原因 |
|------|------|------|
| 框架类型 | `new DistributedCacheEntryOptions { }` | 非业务实体 |
| 简单 Result 类型 | `new ValidationResult { IsValid = true }` | 无复杂映射需求 |
| 查询模型 | `new UserModel { Username = "test" }` | 用于 Repository 查询条件 |
| 配置对象 | `new DestinationConfig { Address = url }` | 框架配置 |
| 内部临时对象 | `new MetricSample { Value = 100 }` | 仅在方法内部使用 |

## 🧪 单元测试

静态 Mappers 的优势之一是**不需要 Mock IMapper**：

```csharp
// ❌ 旧风格需要 Mock
_mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
    .Returns((User u) => new UserDto { Id = u.Id, Username = u.Username });

// ✅ 新风格直接使用
var user = new User { Id = Guid.NewGuid(), Username = "test" };
var dto = user.ToDto();  // 直接调用，无需 Mock
Assert.Equal(user.Id, dto.Id);
```

## 📋 检查清单

改造现有代码时，确保完成以下步骤：

- [ ] 在 Profile 文件中添加 `{Entity}Mappers` 静态类
- [ ] 添加 `ToDto()` / `ToDtos()` 扩展方法
- [ ] 添加 `ToEntity()` 扩展方法（用于 Create）
- [ ] 添加 `ApplyUpdate()` 扩展方法（用于 Update，可选）
- [ ] 在 Service 中添加 `using` 引用 Mapping 命名空间
- [ ] 移除 Service 构造函数中的 `IMapper` 参数
- [ ] 替换所有 `_mapper.Map<>` 为扩展方法调用
- [ ] 运行 `dotnet build` 验证编译通过
- [ ] 更新相关单元测试（移除 IMapper Mock）
