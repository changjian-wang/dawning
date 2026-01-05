---
description: 编写单元测试
---

# 编写单元测试

为指定的代码编写全面的单元测试。

## 测试框架

- **xUnit** - 测试框架
- **Moq** - Mock 框架
- **FluentAssertions** - 断言库（可选）

## 测试文件组织

```
tests/
├── Dawning.Core.Tests/
│   ├── StringExtensionsTests.cs
│   └── JsonExtensionsTests.cs
├── Dawning.Extensions.Tests/
│   └── CollectionExtensionsTests.cs
└── Dawning.Gateway.Tests/
    ├── Services/
    │   └── UserServiceTests.cs
    └── Controllers/
        └── UserControllerTests.cs
```

## 测试方法命名规范

```
{MethodName}_{StateUnderTest}_{ExpectedBehavior}
```

示例：
- `GetById_ValidId_ReturnsUser`
- `Create_NullInput_ThrowsArgumentNullException`
- `Truncate_StringLongerThanMax_ReturnsTruncatedWithSuffix`

## 测试模板

### 基础测试
```csharp
namespace Dawning.{Package}.Tests;

public class {ClassName}Tests
{
    [Fact]
    public void {MethodName}_{Scenario}_{Expected}()
    {
        // Arrange
        var input = "test value";
        
        // Act
        var result = input.{MethodName}();
        
        // Assert
        Assert.Equal("expected", result);
    }
}
```

### 参数化测试
```csharp
[Theory]
[InlineData(null, true)]
[InlineData("", true)]
[InlineData("  ", true)]
[InlineData("hello", false)]
public void IsNullOrWhiteSpace_VariousInputs_ReturnsExpected(string? input, bool expected)
{
    var result = input.IsNullOrWhiteSpace();
    Assert.Equal(expected, result);
}
```

### 异常测试
```csharp
[Fact]
public void Create_NullInput_ThrowsArgumentNullException()
{
    // Arrange
    CreateUserDto? dto = null;
    
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => _service.Create(dto!));
}

[Fact]
public async Task CreateAsync_NullInput_ThrowsArgumentNullException()
{
    // Arrange
    CreateUserDto? dto = null;
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(dto!));
}
```

### Service 测试（带 Mock）

> ⚠️ **重要**: Service 通过 UnitOfWork 访问 Repository，测试时需要通过 UnitOfWork Mock 设置 Repository Mock

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        // ✅ 正确：通过 UnitOfWork 设置 Repository Mock
        _unitOfWorkMock.Setup(x => x.User).Returns(_userRepositoryMock.Object);
        
        _service = new UserService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Username = "test" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                           .ReturnsAsync(expectedUser);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test", result.Username);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                           .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsNewId()
    {
        // Arrange
        var dto = new CreateUserDto { Username = "newuser" };
        var expectedId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<User>()))
                           .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        _userRepositoryMock.Verify(r => r.InsertAsync(It.Is<User>(u => u.Username == "newuser")), Times.Once);
    }
}
```

### ❌ 错误示例（不要这样做）
```csharp
// ❌ 错误：直接注入 Repository Mock 到 Service
public UserServiceTests()
{
    _userRepositoryMock = new Mock<IUserRepository>();
    _service = new UserService(_userRepositoryMock.Object);  // 错误！
}

// ❌ 错误：Mock IMapper（应使用静态 Mapper）
_mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
           .Returns(new UserDto { ... });  // 不需要 Mock，直接用 ToDto()
```

### Controller 测试
```csharp
public class UserControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UserController(_mockService.Object);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto { Id = userId, Username = "test" };
        _mockService.Setup(s => s.GetByIdAsync(userId))
                    .ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResult = Assert.IsType<ApiResult<UserDto>>(okResult.Value);
        Assert.True(apiResult.Success);
        Assert.Equal(userId, apiResult.Data?.Id);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(userId))
                    .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
```

## 测试场景覆盖

### 必须覆盖
- ✅ 正常场景（Happy Path）
- ✅ 边界条件（空值、空集合、最大/最小值）
- ✅ 异常情况（无效输入、不存在的资源）
- ✅ 权限检查（未授权、无权限）

### 建议覆盖
- 并发场景
- 超时场景
- 事务回滚

## 覆盖率目标

| 类型 | 目标覆盖率 |
|------|-----------|
| SDK 包 | > 80% |
| 核心业务逻辑 | > 70% |
| 工具类/扩展方法 | > 90% |
| Controller | > 60% |

## 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定项目测试
dotnet test tests/Dawning.Core.Tests

# 带覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```
