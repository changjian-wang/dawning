---
description: "Use when: Creating xUnit unit tests with Moq, UnitOfWork mock pattern for Service tests, Controller tests with mocked services\nDon't use when: Writing production code (use code-patterns or create-api), running existing tests\nInputs: Class or method to test\nOutputs: Test class with arrange/act/assert pattern, proper mocking of UnitOfWork and repositories\nSuccess criteria: Tests follow naming convention, mock UnitOfWork correctly, cover happy path and edge cases"
---

# Create Tests Skill

## 测试框架

- **xUnit** — 测试框架
- **Moq** — Mock 框架
- **FluentAssertions** — 断言库（可选）

## 命名规范

```
{MethodName}_{StateUnderTest}_{ExpectedBehavior}
```

示例：`GetById_ValidId_ReturnsUser`, `Create_NullInput_ThrowsArgumentNullException`

## Service 测试模板（重点）

> **核心规则**：通过 UnitOfWork Mock 设置 Repository Mock，不直接注入 Repository

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

        // ✅ 通过 UnitOfWork 设置 Repository Mock
        _unitOfWorkMock.Setup(x => x.User).Returns(_userRepositoryMock.Object);

        _service = new UserService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "test" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _service.GetByIdAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
```

### ❌ 错误做法

```csharp
// ❌ 直接注入 Repository Mock
_service = new UserService(_userRepositoryMock.Object);

// ❌ Mock IMapper（应使用静态 Mapper）
_mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>())).Returns(new UserDto());
```

## Controller 测试模板

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
        var userId = Guid.NewGuid();
        var user = new UserDto { Id = userId, Username = "test" };
        _mockService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _controller.GetById(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResult = Assert.IsType<ApiResult<UserDto>>(okResult.Value);
        Assert.True(apiResult.Success);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserDto?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
```

## 其他测试模式

### 参数化测试

```csharp
[Theory]
[InlineData(null, true)]
[InlineData("", true)]
[InlineData("hello", false)]
public void IsNullOrEmpty_VariousInputs_ReturnsExpected(string? input, bool expected)
{
    var result = string.IsNullOrEmpty(input);
    Assert.Equal(expected, result);
}
```

### 异常测试

```csharp
[Fact]
public async Task CreateAsync_NullInput_ThrowsArgumentNullException()
{
    await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
}
```

## 覆盖率目标

| 类型 | 目标 |
|------|------|
| SDK 包 | > 80% |
| 核心业务逻辑 | > 70% |
| 工具类/扩展方法 | > 90% |
| Controller | > 60% |

## 运行命令

```bash
dotnet test                                      # 全部测试
dotnet test tests/Dawning.Core.Tests             # 特定项目
dotnet test --collect:"XPlat Code Coverage"      # 带覆盖率
```

