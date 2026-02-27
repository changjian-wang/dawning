---
description: "Create SDK package features for Dawning: extension methods, services, middleware. Trigger: SDK, 扩展方法, extension, 中间件, middleware, NuGet包, package, Dawning.Core, Dawning.Extensions"
---

# Create SDK Feature Skill

## 目标

为 Dawning SDK 包添加新功能、扩展方法或中间件。

## 触发条件

- **关键词**：SDK, 扩展方法, extension, 中间件, middleware, NuGet包, package, 新功能
- **文件模式**：`sdk/**/*.cs`, `Dawning.Core/**`, `Dawning.Extensions/**`
- **用户意图**：创建 SDK 功能、添加扩展方法、创建中间件

## 编排

- **前置**：无
- **后续**：`create-tests`（SDK 包必须 >80% 覆盖率）

---

## SDK 包结构

| 包 | 用途 |
|----|------|
| `Dawning.Core` | 异常、中间件、统一结果 |
| `Dawning.Extensions` | 字符串、集合、JSON 扩展方法 |
| `Dawning.Identity` | JWT 解析、用户上下文 |
| `Dawning.Caching` | 内存缓存、Redis |
| `Dawning.Logging` | 结构化日志、请求追踪 |
| `Dawning.Messaging` | RabbitMQ、Azure Service Bus |
| `Dawning.ORM.Dapper` | Dapper 增强、分页 |
| `Dawning.Resilience` | 重试、熔断、超时 |

## 扩展方法模板

```csharp
namespace Dawning.Extensions;

/// <summary>
/// {Type} 扩展方法
/// </summary>
public static class {Type}Extensions
{
    /// <summary>
    /// {方法描述}
    /// </summary>
    /// <example>
    /// <code>
    /// var result = "hello".ToTitleCase();
    /// </code>
    /// </example>
    public static string ToTitleCase(this string source)
    {
        if (string.IsNullOrEmpty(source)) return source;
        return char.ToUpper(source[0]) + source[1..];
    }
}
```

## 服务接口模板

```csharp
namespace Dawning.{Package};

public interface I{Service}Service
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
}
```

## 中间件模板

```csharp
namespace Dawning.{Package}.Middleware;

public class {Name}Middleware(RequestDelegate next, ILogger<{Name}Middleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 前置处理
        await next(context);
        // 后置处理
    }
}

public static class {Name}Extensions
{
    public static IApplicationBuilder Use{Name}(this IApplicationBuilder app)
        => app.UseMiddleware<{Name}Middleware>();
}
```

## 规范要求

- ✅ 使用 file-scoped namespaces
- ✅ 完整的 XML 文档注释（含 `<example>`）
- ✅ 处理 null 参数
- ✅ 单元测试覆盖率 > 80%
- ✅ 测试命名：`{MethodName}_{State}_{Expected}`

## 验收场景

- **输入**："给 Dawning.Extensions 添加一个字符串截断方法"
- **预期**：agent 生成扩展方法 + XML 文档 + 单元测试
- **上次验证**：2026-02-27 ✅
