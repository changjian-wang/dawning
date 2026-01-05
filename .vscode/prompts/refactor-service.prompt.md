---
description: Service 层重构指南
---

# Service 层重构指南

将现有 Service 代码重构为符合项目规范的模式。

## 重构检查清单

### 依赖注入重构
- [ ] 移除直接注入的 `IMapper`，改用静态 Mapper
- [ ] 移除冗余的 Repository 注入，通过 UnitOfWork 访问
- [ ] 字段命名从 `_uow` 改为 `_unitOfWork`
- [ ] 使用 primary constructor (C# 12)

### 对象映射重构
- [ ] 创建对应的 `{Entity}Mappers` 静态类
- [ ] 添加 `ToDto()`, `ToDtos()`, `ToEntity()`, `ApplyUpdate()` 扩展方法
- [ ] 替换所有 `_mapper.Map<>()` 调用

### 异常处理重构
- [ ] 使用具体异常类型替代通用 Exception
- [ ] 使用常量替代硬编码字符串

## 重构示例

### Before（重构前）

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;  // ❌ 冗余
    private readonly IUnitOfWork _uow;                 // ❌ 命名
    private readonly IMapper _mapper;                  // ❌ 不应注入
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,               // ❌ 冗余
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? _mapper.Map<UserDto>(user) : null;  // ❌ 使用 IMapper
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = _mapper.Map<User>(dto);  // ❌ 使用 IMapper
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        
        await _userRepository.InsertAsync(user);
        
        // ❌ 硬编码字符串
        _logger.LogInformation("User created: {Username}", user.Username);
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new Exception("User not found");  // ❌ 通用异常

        _mapper.Map(dto, user);  // ❌ 使用 IMapper
        user.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user);
    }
}
```

### After（重构后）

```csharp
using YourProject.Application.Mapping;
using YourProject.Domain.Constants;
using YourProject.Core.Exceptions;

namespace YourProject.Application.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public class UserService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger) : IUserService
{
    // ✅ 使用 primary constructor，无需显式字段声明
    // ✅ 只注入 UnitOfWork，通过它访问 Repository
    // ✅ 不注入 IMapper，使用静态 Mapper

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await unitOfWork.User.GetByIdAsync(id);
        return user?.ToDto();  // ✅ 静态 Mapper 扩展方法
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, string? username = null)
    {
        // ✅ 业务验证
        if (await unitOfWork.User.ExistsByUsernameAsync(dto.Username))
            throw new ConflictException($"Username '{dto.Username}' already exists", "USERNAME_EXISTS");

        var user = dto.ToEntity();  // ✅ 静态 Mapper
        user.CreatedBy = username;
        
        await unitOfWork.User.InsertAsync(user);
        
        logger.LogInformation("User {UserId} created by {Operator}", user.Id, username);
        
        return user.ToDto();  // ✅ 静态 Mapper
    }

    public async Task<UserDto?> UpdateAsync(UpdateUserDto dto, string? username = null)
    {
        var user = await unitOfWork.User.GetByIdAsync(dto.Id);
        if (user == null)
            return null;  // ✅ 返回 null 让 Controller 处理 404

        user.ApplyUpdate(dto);  // ✅ 静态 Mapper
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
        UserQueryModel query,
        int page,
        int pageSize)
    {
        var pagedData = await unitOfWork.User.GetPagedListAsync(query, page, pageSize);
        return new PagedData<UserDto>
        {
            Items = pagedData.Items.ToDtos(),  // ✅ 批量转换
            TotalCount = pagedData.TotalCount,
            PageIndex = pagedData.PageIndex,
            PageSize = pagedData.PageSize
        };
    }
}
```

## Mapper 类创建

为每个 Entity 创建对应的 Mapper：

```csharp
namespace YourProject.Application.Mapping;

/// <summary>
/// User AutoMapper 配置
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => 
                opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

/// <summary>
/// User 静态映射器
/// </summary>
public static class UserMappers
{
    private static IMapper Mapper { get; }

    static UserMappers()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        Mapper = config.CreateMapper();
    }

    public static UserDto ToDto(this User entity) => Mapper.Map<UserDto>(entity);
    public static UserDto? ToDtoOrNull(this User? entity) => entity?.ToDto();
    public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> entities) => 
        entities.Select(e => e.ToDto());
    public static User ToEntity(this CreateUserDto dto) => Mapper.Map<User>(dto);
    public static void ApplyUpdate(this User entity, UpdateUserDto dto) => Mapper.Map(dto, entity);
}
```

## 单元测试更新

重构后需要更新对应的单元测试：

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UserService>>();
        
        // ✅ 通过 UnitOfWork 设置 Repository Mock
        _unitOfWorkMock.Setup(x => x.User).Returns(_userRepositoryMock.Object);
        
        // ✅ 只传入 UnitOfWork（不传 IMapper）
        _service = new UserService(
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "test" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                           .ReturnsAsync(user);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        // ✅ 静态 Mapper 自动工作，无需 Mock
    }
}
```

## 重构步骤

1. **创建 Mapper 类**（如不存在）
2. **更新 Service 构造函数**
   - 移除 `IMapper` 参数
   - 移除冗余 Repository 参数
   - 改用 primary constructor
3. **替换 Mapper 调用**
   - `_mapper.Map<Dto>(entity)` → `entity.ToDto()`
   - `_mapper.Map<Entity>(dto)` → `dto.ToEntity()`
   - `_mapper.Map(dto, entity)` → `entity.ApplyUpdate(dto)`
4. **替换 Repository 调用**
   - `_userRepository.XXX()` → `unitOfWork.User.XXX()`
5. **更新异常处理**
   - 使用具体异常类型
   - 使用常量替代硬编码字符串
6. **更新单元测试**
   - 移除 `IMapper` Mock
   - 通过 UnitOfWork 设置 Repository Mock
7. **验证**
   - 运行 `dotnet build`
   - 运行 `dotnet test`
