# Dawning 共享组件库集成指南

本文档介绍如何在新服务中集成 Dawning 共享组件库。

## 📦 可用的共享库

| 库名 | 用途 | NuGet/项目引用 |
|------|------|----------------|
| `Dawning.Shared.Authentication` | 统一认证集成 | [详见认证文档](AUTHENTICATION_INTEGRATION.md) |
| `Dawning.Shared.Core` | 核心组件（异常、响应、分页） | 本文档 |
| `Dawning.Shared.Logging` | 统一日志配置 | 本文档 |

## 🚀 快速开始

### 1. 添加项目引用

```xml
<ItemGroup>
  <ProjectReference Include="..\Shared\Dawning.Shared.Core\Dawning.Shared.Core.csproj" />
  <ProjectReference Include="..\Shared\Dawning.Shared.Logging\Dawning.Shared.Logging.csproj" />
  <ProjectReference Include="..\Shared\Dawning.Shared.Authentication\Dawning.Shared.Authentication.csproj" />
</ItemGroup>
```

### 2. 配置日志 (Program.cs)

```csharp
using Dawning.Shared.Logging.Extensions;
using Serilog;

// 创建启动日志器（捕获启动错误）
Log.Logger = LoggingExtensions.CreateBootstrapLogger("MyService");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 使用 Dawning 统一日志配置
    builder.Host.UseDawningLogging(options =>
    {
        options.ApplicationName = "MyService";
        options.MinimumLevel = LogLevel.Information;
        options.EnableFile = true;
        options.LogFilePath = "logs/myservice-.log";
    });

    // ... 其他配置
    
    var app = builder.Build();
    
    // 使用日志富化中间件
    app.UseDawningLoggingEnrichment();
    
    // ... 其他中间件
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
```

### 3. 配置异常处理 (Program.cs)

```csharp
using Dawning.Shared.Core.Extensions;

var app = builder.Build();

// 使用全局异常处理（应该在最外层）
app.UseDawningExceptionHandling();

// 使用日志富化
app.UseDawningLoggingEnrichment();

// ... 其他中间件
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 📋 Core 库详解

### 异常类型

| 异常类型 | HTTP 状态码 | 用途 |
|----------|-------------|------|
| `BusinessException` | 400 | 通用业务异常基类 |
| `NotFoundException` | 404 | 资源未找到 |
| `UnauthorizedException` | 401 | 未授权访问 |
| `ForbiddenException` | 403 | 禁止访问 |
| `ValidationException` | 400 | 验证失败 |
| `ConflictException` | 409 | 数据冲突（如重复） |
| `TooManyRequestsException` | 429 | 请求过多（限流） |
| `ServiceUnavailableException` | 503 | 服务不可用 |

### 使用异常

```csharp
using Dawning.Shared.Core.Exceptions;

public class UserService
{
    public async Task<User> GetUserAsync(int id)
    {
        var user = await _repository.FindAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);
        
        return user;
    }

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        // 验证异常
        if (string.IsNullOrEmpty(dto.Email))
            throw new ValidationException("Email", "Email is required");

        // 检查重复
        if (await _repository.ExistsByEmailAsync(dto.Email))
            throw new ConflictException("User", "Email", dto.Email);

        // 权限检查
        if (!_currentUser.HasPermission("user:create"))
            throw new ForbiddenException("User", "create");

        // ... 创建用户
    }
}
```

### 统一响应格式 (ApiResult)

```csharp
using Dawning.Shared.Core.Results;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<UserDto>>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(ApiResults.Ok(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResult<UserDto>>> CreateUser(CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(
            nameof(GetUser), 
            new { id = user.Id }, 
            ApiResults.Created(user));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult>> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResults.Ok("User deleted successfully"));
    }
}
```

### 响应格式示例

成功响应：
```json
{
  "success": true,
  "code": "OK",
  "message": null,
  "data": { "id": 1, "name": "John" },
  "timestamp": 1699876543210,
  "traceId": "abc123"
}
```

错误响应：
```json
{
  "success": false,
  "code": "NOT_FOUND",
  "message": "User with id '999' was not found",
  "data": null,
  "timestamp": 1699876543210,
  "traceId": "abc123"
}
```

验证错误响应：
```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "Validation failed",
  "data": {
    "email": ["Email is required", "Email format is invalid"],
    "password": ["Password must be at least 8 characters"]
  },
  "timestamp": 1699876543210,
  "traceId": "abc123"
}
```

### 分页

```csharp
using Dawning.Shared.Core.Results;

[HttpGet]
public async Task<ActionResult<ApiResult<PagedResult<UserDto>>>> GetUsers(
    [FromQuery] PagedRequest request)
{
    var users = await _userService.GetUsersAsync(request);
    return Ok(ApiResults.Ok(users));
}

// Service 实现
public async Task<PagedResult<UserDto>> GetUsersAsync(PagedRequest request)
{
    var query = _context.Users.AsQueryable();
    
    // 搜索
    if (!string.IsNullOrEmpty(request.Keyword))
    {
        query = query.Where(u => u.Name.Contains(request.Keyword));
    }
    
    // 排序
    if (!string.IsNullOrEmpty(request.SortField))
    {
        query = request.SortDescending 
            ? query.OrderByDescending(u => EF.Property<object>(u, request.SortField))
            : query.OrderBy(u => EF.Property<object>(u, request.SortField));
    }
    
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip(request.Skip)
        .Take(request.Take)
        .Select(u => u.ToDto())
        .ToListAsync();
    
    return new PagedResult<UserDto>(items, totalCount, request.PageIndex, request.PageSize);
}
```

## 📝 Logging 库详解

### 日志配置选项

```csharp
builder.Host.UseDawningLogging(options =>
{
    // 应用名称
    options.ApplicationName = "MyService";
    
    // 最小日志级别
    options.MinimumLevel = LogLevel.Information;
    
    // 控制台输出
    options.EnableConsole = true;
    
    // 文件输出
    options.EnableFile = true;
    options.LogFilePath = "logs/myservice-.log";  // 日期会自动添加
    options.RetainedFileCountLimit = 31;          // 保留31天
    options.FileSizeLimitMb = 50;                 // 单文件50MB
    options.RollOnFileSizeLimit = true;           // 超限滚动
    
    // JSON 格式（适合日志收集系统）
    options.UseJsonFormat = false;
    
    // 覆盖特定命名空间的日志级别
    options.OverrideMinimumLevels["Microsoft"] = LogLevel.Warning;
    options.OverrideMinimumLevels["Microsoft.EntityFrameworkCore"] = LogLevel.Warning;
});
```

### 日志上下文

使用 `UseDawningLoggingEnrichment()` 中间件后，日志会自动包含：

| 属性 | 说明 |
|------|------|
| `TraceId` | 请求追踪 ID |
| `UserId` | 当前用户 ID |
| `UserName` | 当前用户名 |
| `ClientIp` | 客户端 IP |
| `UserAgent` | 用户代理 |
| `TenantId` | 租户 ID（多租户） |
| `RequestPath` | 请求路径 |
| `RequestMethod` | 请求方法 |

### 使用日志

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<User> GetUserAsync(int id)
    {
        _logger.LogInformation("Getting user {UserId}", id);
        
        var user = await _repository.FindAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException("User", id);
        }
        
        return user;
    }
}
```

日志输出示例：
```
[14:23:45 INF] Dawning.Services.UserService
      Getting user 123
      TraceId: abc123, UserId: admin, ClientIp: 192.168.1.1
```

## 🔧 完整配置示例

```csharp
using Dawning.Shared.Authentication.Extensions;
using Dawning.Shared.Core.Extensions;
using Dawning.Shared.Logging.Extensions;
using Serilog;

Log.Logger = LoggingExtensions.CreateBootstrapLogger("OrderService");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 日志
    builder.Host.UseDawningLogging(options =>
    {
        options.ApplicationName = "OrderService";
        options.EnableFile = true;
    });

    // 认证
    builder.Services.AddDawningAuthentication(options =>
    {
        options.Authority = "https://localhost:5202";
        options.ValidateLifetime = true;
    });

    // 其他服务...
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // 中间件顺序很重要！
    app.UseDawningExceptionHandling();  // 1. 最外层异常处理
    app.UseDawningLoggingEnrichment();  // 2. 日志上下文富化
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseAuthentication();            // 3. 认证
    app.UseAuthorization();             // 4. 授权
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
```

## 📁 项目结构建议

```
MyNewService/
├── src/
│   └── MyNewService.Api/
│       ├── Controllers/
│       ├── Services/
│       ├── Models/
│       ├── Program.cs
│       └── MyNewService.Api.csproj
├── tests/
│   └── MyNewService.Tests/
└── README.md
```

## 🔗 相关文档

- [认证集成指南](AUTHENTICATION_INTEGRATION.md)
- [API 网关配置](../../Dawning.Gateway.Api/README.md)
- [身份服务文档](../IDENTITY_API.md)
