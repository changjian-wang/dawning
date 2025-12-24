# 示例代码

本文档提供 Dawning SDK 各个包的使用示例。

## Dawning.Core 示例

### API 响应封装

```csharp
using Dawning.Core;

// 成功响应
var successResponse = ApiResponse<User>.Success(user, "获取用户成功");
// {
//   "success": true,
//   "message": "获取用户成功",
//   "data": { "id": 1, "name": "张三" }
// }

// 失败响应
var errorResponse = ApiResponse<User>.Fail("用户不存在");
// {
//   "success": false,
//   "message": "用户不存在",
//   "data": null
// }

// 分页响应
var pagedResponse = PagedResponse<User>.Create(users, page: 1, pageSize: 10, total: 100);
```

### 业务异常

```csharp
using Dawning.Core.Exceptions;

// 抛出业务异常
throw new BusinessException("订单已过期，无法取消");

// 带错误码的业务异常
throw new BusinessException("ORDER_EXPIRED", "订单已过期");

// 验证异常
throw new ValidationException("邮箱格式不正确");

// 未授权异常
throw new UnauthorizedException("请先登录");

// 资源未找到
throw new NotFoundException("用户", userId);
```

## Dawning.Extensions 示例

### 字符串扩展

```csharp
using Dawning.Extensions;

// 大小写转换
"getUserInfo".ToPascalCase();    // "GetUserInfo"
"GetUserInfo".ToCamelCase();     // "getUserInfo"
"getUserInfo".ToSnakeCase();     // "get_user_info"
"getUserInfo".ToKebabCase();     // "get-user-info"

// 验证
"test@example.com".IsValidEmail();        // true
"13800138000".IsValidPhoneNumber();       // true
"".IsNullOrEmpty();                       // true

// 脱敏
"13800138000".Mask();                     // "138****8000"
"test@example.com".Mask();                // "te**@example.com"
"张三".Mask();                            // "张*"

// 截断
"这是一段很长的文本".Truncate(5);          // "这是一段很..."
```

### 集合扩展

```csharp
using Dawning.Extensions;

var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// 分批处理
foreach (var batch in list.Batch(3))
{
    Console.WriteLine(string.Join(", ", batch));
    // 输出: 1, 2, 3
    // 输出: 4, 5, 6
    // 输出: 7, 8, 9
    // 输出: 10
}

// 去重
var users = new List<User> { ... };
var distinctUsers = users.DistinctBy(u => u.Email);

// 安全遍历
users.ForEach(u => Console.WriteLine(u.Name));

// 随机元素
var random = list.RandomElement();

// 打乱顺序
var shuffled = list.Shuffle();
```

### 日期时间扩展

```csharp
using Dawning.Extensions;

var now = DateTime.Now;

// 时间边界
now.StartOfDay();      // 2025-12-24 00:00:00
now.EndOfDay();        // 2025-12-24 23:59:59
now.StartOfMonth();    // 2025-12-01 00:00:00
now.EndOfMonth();      // 2025-12-31 23:59:59

// 判断
now.IsWeekend();       // false (周二)
now.IsWorkday();       // true
now.IsToday();         // true

// 计算
var birthday = new DateTime(1990, 5, 15);
birthday.CalculateAge();  // 35

// 相对时间
DateTime.Now.AddMinutes(-5).ToRelativeTime();  // "5分钟前"
DateTime.Now.AddHours(-2).ToRelativeTime();    // "2小时前"
DateTime.Now.AddDays(-1).ToRelativeTime();     // "昨天"
```

### JSON 扩展

```csharp
using Dawning.Extensions;

// 序列化
var user = new User { Id = 1, Name = "张三" };
var json = user.ToJson();
var prettyJson = user.ToJson(indented: true);

// 反序列化
var user2 = json.FromJson<User>();

// 深拷贝
var clone = user.DeepClone();

// 验证
"{\"name\": \"test\"}".IsValidJson();  // true
"invalid json".IsValidJson();          // false
```

### 对象扩展

```csharp
using Dawning.Extensions;

// 空值检查
object obj = null;
obj.IsNull();     // true
obj.IsNotNull();  // false

// 默认值
string name = null;
name.IfNull("默认名称");  // "默认名称"

// 范围限制
15.Clamp(0, 10);   // 10
-5.Clamp(0, 10);   // 0
5.Clamp(0, 10);    // 5

// 范围检查
5.IsBetween(1, 10);   // true
15.IsBetween(1, 10);  // false

// 枚举描述
MyEnum.Value.GetDescription();  // 获取 [Description] 特性值

// 反射
user.GetPropertyValue("Name");  // "张三"
user.SetPropertyValue("Name", "李四");
```

## Dawning.Identity 示例

### JWT 认证

```csharp
using Dawning.Identity;

// 生成 Token
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, userId.ToString()),
    new(ClaimTypes.Name, userName),
    new(ClaimTypes.Role, "Admin")
};
var token = _jwtService.GenerateToken(claims);

// 获取当前用户
public class UserController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    
    public UserController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            Id = _currentUser.Id,
            UserName = _currentUser.UserName,
            Roles = _currentUser.Roles
        });
    }
}
```

## Dawning.Resilience 示例

### 弹性 HTTP 客户端

```csharp
using Dawning.Resilience;

// 注册服务
builder.Services.AddResilientHttpClient("payment-api", options =>
{
    options.BaseAddress = new Uri("https://payment.example.com");
    options.RetryCount = 3;              // 重试 3 次
    options.CircuitBreakerThreshold = 5; // 5 次失败后熔断
    options.TimeoutSeconds = 30;         // 30 秒超时
});

// 使用
public class PaymentService
{
    private readonly HttpClient _httpClient;
    
    public PaymentService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("payment-api");
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/payments", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaymentResult>();
    }
}
```

## Dawning.Logging 示例

### Serilog 配置

```csharp
using Dawning.Logging;

var builder = WebApplication.CreateBuilder(args);

// 添加 Serilog（自动从配置读取）
builder.AddDawningSerilog();

// 或者使用自定义配置
builder.AddDawningSerilog(config =>
{
    config.MinimumLevel.Information()
          .WriteTo.Console()
          .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day);
});

// 使用结构化日志
Log.Information("用户 {UserId} 登录成功", userId);
Log.Warning("API 调用失败: {StatusCode}", response.StatusCode);
Log.Error(ex, "处理订单 {OrderId} 时发生错误", orderId);
```

## Dawning.ORM.Dapper 示例

### Dapper 扩展

```csharp
using Dawning.ORM.Dapper;

public class UserRepository
{
    private readonly IDbConnection _connection;
    
    // 获取单个实体
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _connection.GetAsync<User>(id);
    }
    
    // 获取所有实体
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _connection.GetAllAsync<User>();
    }
    
    // 插入实体
    public async Task<int> CreateAsync(User user)
    {
        return await _connection.InsertAsync(user);
    }
    
    // 更新实体
    public async Task<bool> UpdateAsync(User user)
    {
        return await _connection.UpdateAsync(user);
    }
    
    // 删除实体
    public async Task<bool> DeleteAsync(int id)
    {
        return await _connection.DeleteAsync<User>(id);
    }
}
```

## 完整示例项目

查看 SDK 仓库中的示例项目：

- [WebApiSample](https://github.com/changjian-wang/dawning/tree/main/sdk/samples/WebApiSample) - ASP.NET Core Web API 示例
- [ConsoleSample](https://github.com/changjian-wang/dawning/tree/main/sdk/samples/ConsoleSample) - 控制台应用示例
