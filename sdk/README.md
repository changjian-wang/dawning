# Dawning SDK

Dawning 项目的公共组件库，提供微服务开发的通用功能。

## 安装

### 配置 NuGet 源

```bash
dotnet nuget add source "https://nuget.pkg.github.com/changjian-wang/index.json" \
  --name "github" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT
```

### 安装包

```bash
dotnet add package Dawning.Core --version 1.0.0
dotnet add package Dawning.Identity --version 1.0.0
dotnet add package Dawning.Logging --version 1.0.0
dotnet add package Dawning.ORM.Dapper --version 1.0.0
dotnet add package Dawning.Resilience --version 1.0.0
dotnet add package Dawning.Extensions --version 1.0.0
```

---

## 可用包

| 包名 | 描述 |
|------|------|
| Dawning.Core | 业务异常、统一 API 响应、异常处理中间件 |
| Dawning.Identity | JWT 认证集成、用户上下文、Claims 扩展 |
| Dawning.Logging | Serilog 集成、结构化日志、请求日志中间件 |
| Dawning.ORM.Dapper | Dapper CRUD 扩展方法 |
| Dawning.Resilience | 重试策略、熔断器、超时处理 (基于 Polly) |
| Dawning.Extensions | 通用扩展方法（字符串、集合、日期、JSON、对象） |

---

## 使用指南

### 1. Dawning.Core

统一 API 响应和异常处理。

```csharp
using Dawning.Core.Results;
using Dawning.Core.Exceptions;
using Dawning.Core.Extensions;

// Program.cs - 注册异常处理中间件
app.UseDawningExceptionHandling();

// Controller 返回统一响应
[HttpGet]
public ApiResult<UserDto> GetUser(int id)
{
    var user = _userService.GetById(id);
    return ApiResult<UserDto>.Success(user);
}

// 抛出业务异常
if (user == null)
    throw new NotFoundException("用户不存在");

// 分页响应
return PagedResult<UserDto>.Success(items, total, page, pageSize);
```

**可用异常类型：**
- `BusinessException` - 通用业务异常 (400)
- `NotFoundException` - 资源不存在 (404)
- `UnauthorizedException` - 未授权 (401)
- `ForbiddenException` - 禁止访问 (403)
- `ValidationException` - 验证失败 (400)

---

### 2. Dawning.Identity

JWT 认证和用户上下文。

```csharp
using Dawning.Identity;
using Dawning.Identity.Extensions;

// Program.cs - 注册服务
builder.Services.AddDawningAuthentication(options =>
{
    options.Authority = "https://your-identity-server";
    options.Audience = "your-api";
});

// 注入当前用户
public class MyService
{
    private readonly CurrentUser _currentUser;

    public MyService(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void DoSomething()
    {
        var userId = _currentUser.Id;
        var userName = _currentUser.UserName;
        var tenantId = _currentUser.TenantId;
        var roles = _currentUser.Roles;
    }
}

// Claims 扩展
var userId = User.GetUserId();
var tenantId = User.GetTenantId();
bool isAdmin = User.HasRole("Admin");
```

---

### 3. Dawning.Logging

Serilog 结构化日志。

```csharp
using Dawning.Logging.Extensions;

// Program.cs
builder.AddDawningLogging(options =>
{
    options.ApplicationName = "MyService";
    options.EnableConsole = true;
    options.EnableFile = true;
    options.FilePath = "logs/app-.log";
    options.SeqServerUrl = "http://localhost:5341"; // 可选
});

// 使用日志丰富中间件
app.UseDawningLogEnrichment();
```

---

### 4. Dawning.ORM.Dapper

Dapper CRUD 扩展。

```csharp
using Dawning.ORM.Dapper;

// 实体定义
[Table("users")]
public class User
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// CRUD 操作
using var connection = new NpgsqlConnection(connectionString);

// 插入
var id = await connection.InsertAsync(new User { Name = "张三", Email = "test@example.com" });

// 查询
var user = await connection.GetAsync<User>(id);
var allUsers = await connection.GetAllAsync<User>();

// 更新
user.Name = "李四";
await connection.UpdateAsync(user);

// 删除
await connection.DeleteAsync(user);
```

---

### 5. Dawning.Resilience

弹性策略（重试、熔断、超时）。

```csharp
using Dawning.Resilience.Extensions;
using Dawning.Resilience.Options;
using Dawning.Resilience.Policies;

// 方式1: 注册弹性服务
builder.Services.AddDawningResilience(options =>
{
    // 重试配置
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BaseDelayMs = 200;
    options.Retry.UseExponentialBackoff = true;
    
    // 熔断器配置
    options.CircuitBreaker.FailureRatioThreshold = 0.5;
    options.CircuitBreaker.SamplingDurationSeconds = 30;
    options.CircuitBreaker.BreakDurationSeconds = 30;
    
    // 超时配置
    options.Timeout.TimeoutSeconds = 30;
});

// 方式2: 添加弹性 HttpClient
builder.Services.AddResilientHttpClient<IMyApiClient>(
    client => 
    {
        client.BaseAddress = new Uri("https://api.example.com");
    },
    options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.CircuitBreaker.Enabled = true;
    });

// 方式3: 手动使用策略
public class MyService
{
    private readonly ResiliencePolicyBuilder _policyBuilder;

    public MyService(ResiliencePolicyBuilder policyBuilder)
    {
        _policyBuilder = policyBuilder;
    }

    public async Task<string> CallExternalApiAsync()
    {
        var pipeline = _policyBuilder.Build<string>();
        
        return await pipeline.ExecuteAsync(async () =>
        {
            // 调用外部 API
            return await _httpClient.GetStringAsync("/api/data");
        });
    }
}
```

---

### 6. Dawning.Extensions

通用扩展方法库。

```csharp
using Dawning.Extensions;

// 字符串扩展
"hello_world".ToPascalCase();     // "HelloWorld"
"HelloWorld".ToCamelCase();       // "helloWorld"
"HelloWorld".ToSnakeCase();       // "hello_world"
"test@example.com".IsValidEmail(); // true
"13812345678".Mask();              // "138****5678"
"长字符串".Truncate(5);            // "长字..."

// 集合扩展
list.IsNullOrEmpty();
items.Batch(100);                  // 分批处理
users.DistinctBy(u => u.Email);   // 去重
dict1.Merge(dict2);               // 合并字典
list.JoinToString(", ");          // 连接字符串

// 日期时间扩展
DateTime.Now.StartOfDay();         // 00:00:00
DateTime.Now.EndOfMonth();         // 月末最后一天
dateTime.IsWeekend();             // 是否周末
birthDate.CalculateAge();          // 计算年龄
dateTime.ToRelativeTime();         // "3分钟前"
date.AddWorkdays(5);              // 添加工作日

// JSON 扩展
var json = obj.ToJson();           // 序列化
var obj = json.FromJson<User>();   // 反序列化
obj.DeepClone();                  // 深克隆
json.PrettyPrint();               // 格式化

// 对象扩展
obj.IfNull(defaultValue);
value.IsIn(1, 2, 3);
value.Clamp(0, 100);
MyEnum.Value.GetDescription();
```

---

## 项目结构

```
sdk/
├── Dawning.Core/           # 核心库
├── Dawning.Identity/       # 认证库
├── Dawning.Logging/        # 日志库
├── Dawning.ORM.Dapper/     # ORM 库
├── Dawning.Resilience/     # 弹性库
├── Dawning.Extensions/     # 扩展库
├── Directory.Build.props   # 统一构建配置
└── Dawning.SDK.sln         # 解决方案文件
```

## 开发

```bash
# 构建所有包
cd sdk
dotnet build

# 运行测试
dotnet test

# 打包
dotnet pack -c Release -o ./nupkgs

# 发布到 GitHub Packages
dotnet nuget push ./nupkgs/*.nupkg --source "github"
```

## 版本历史

### v1.0.0 (2024-12-24)
- 初始版本
- Dawning.Core: 异常处理、API 响应
- Dawning.Identity: JWT 认证、用户上下文
- Dawning.Logging: Serilog 集成
- Dawning.ORM.Dapper: Dapper CRUD 扩展
- Dawning.Resilience: Polly 弹性策略
- Dawning.Extensions: 通用扩展方法
