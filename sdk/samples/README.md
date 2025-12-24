# Dawning SDK Samples

SDK 使用示例项目。

## 项目列表

### 1. WebApiSample

完整的 ASP.NET Core Web API 示例，演示所有 SDK 功能：

- **Dawning.Core** - 异常处理、统一 API 响应
- **Dawning.Identity** - JWT 认证、用户上下文
- **Dawning.Logging** - Serilog 结构化日志
- **Dawning.Resilience** - 重试、熔断、超时策略
- **Dawning.Extensions** - 字符串、集合、日期、JSON 扩展

### 2. ConsoleSample

简单的控制台应用示例，演示 Extensions 和 Core 库的使用。

## 运行示例

```bash
# Web API 示例
cd samples/WebApiSample
dotnet run

# 控制台示例
cd samples/ConsoleSample
dotnet run
```

## API 端点

启动 WebApiSample 后访问 Swagger UI：

```
http://localhost:5000/swagger
```

### 用户管理 API

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | /api/users | 获取用户列表（分页） |
| GET | /api/users/{id} | 获取单个用户 |
| POST | /api/users | 创建用户 |
| GET | /api/users/me | 获取当前登录用户 |

### 外部 API 调用

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | /api/externalapi/posts | 获取帖子列表（弹性策略） |
| GET | /api/externalapi/posts/{id} | 获取单个帖子 |
| POST | /api/externalapi/posts/batch | 批量获取帖子 |

### 扩展方法演示

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | /api/extensions/string | 字符串扩展演示 |
| GET | /api/extensions/datetime | 日期时间扩展演示 |
| POST | /api/extensions/json | JSON 扩展演示 |
| GET | /api/extensions/collection | 集合扩展演示 |
| GET | /api/extensions/object | 对象扩展演示 |
| GET | /api/extensions/mask | 手机号掩码演示 |
| GET | /api/extensions/timestamp | 时间戳工具演示 |

## 代码亮点

### 异常处理

```csharp
// 抛出业务异常，由中间件统一处理
throw new NotFoundException($"用户 {id} 不存在");
throw new ValidationException("邮箱格式不正确");
throw new BusinessException("邮箱已被使用");
```

### 统一响应格式

```csharp
// 成功响应
return Ok(ApiResults.Ok(data));
return Ok(ApiResults.Ok(data, "操作成功"));

// 分页响应
return Ok(PagedResult<T>.FromList(items, total, page, pageSize));
```

### 弹性策略

```csharp
// 方式1: 注入弹性 HttpClient
builder.Services.AddResilientHttpClient<IExternalApiClient, ExternalApiClient>(...);

// 方式2: 手动使用策略
var pipeline = _policyBuilder.Build<T>();
var result = await pipeline.ExecuteAsync(async ct => await DoSomething());
```

### 扩展方法

```csharp
// 字符串
"HelloWorld".ToCamelCase();     // "helloWorld"
"13812345678".Mask();           // "138****5678"
email.IsValidEmail();           // true/false

// 日期
DateTime.Now.StartOfDay();      // 00:00:00
dateTime.ToRelativeTime();      // "3分钟前"

// 集合
items.Batch(100);               // 分批处理
dict1.Merge(dict2);             // 合并字典

// JSON
obj.ToJson();                   // 序列化
json.FromJson<T>();             // 反序列化
```
