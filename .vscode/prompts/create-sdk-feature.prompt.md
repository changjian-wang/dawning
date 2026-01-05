---
description: 创建 SDK 包的新功能或扩展方法
---

# 创建 SDK 功能

为 Dawning SDK 包添加新功能或扩展方法。

## SDK 包结构

```
sdk/
├── Dawning.Core/          # 核心基础库
├── Dawning.Extensions/    # 扩展方法库
├── Dawning.Identity/      # 身份认证库
├── Dawning.Caching/       # 缓存服务
├── Dawning.Logging/       # 日志服务
├── Dawning.Messaging/     # 消息队列
├── Dawning.ORM.Dapper/    # Dapper 增强
├── Dawning.Resilience/    # 弹性策略
└── tests/                 # 单元测试
```

## 创建流程

### 1. 确定目标包
根据功能类型选择合适的包：
- 通用异常/结果 → `Dawning.Core`
- 扩展方法 → `Dawning.Extensions`
- 身份/JWT → `Dawning.Identity`
- 缓存 → `Dawning.Caching`
- 日志 → `Dawning.Logging`
- 消息队列 → `Dawning.Messaging`
- 数据库 → `Dawning.ORM.Dapper`
- 重试/熔断 → `Dawning.Resilience`

### 2. 创建功能代码

#### 扩展方法示例
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
    /// <param name="source">源对象</param>
    /// <returns>处理结果</returns>
    /// <example>
    /// <code>
    /// var result = "hello".ToTitleCase();
    /// // result = "Hello"
    /// </code>
    /// </example>
    public static string ToTitleCase(this string source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        return char.ToUpper(source[0]) + source[1..];
    }
}
```

#### 服务接口示例
```csharp
namespace Dawning.{Package};

/// <summary>
/// {Service} 服务接口
/// </summary>
public interface I{Service}Service
{
    /// <summary>
    /// {方法描述}
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// {方法描述}
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
}
```

#### 中间件示例
```csharp
namespace Dawning.{Package}.Middleware;

/// <summary>
/// {Middleware} 中间件
/// </summary>
public class {Middleware}Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<{Middleware}Middleware> _logger;

    public {Middleware}Middleware(RequestDelegate next, ILogger<{Middleware}Middleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 前置处理
        
        await _next(context);
        
        // 后置处理
    }
}

/// <summary>
/// 中间件扩展
/// </summary>
public static class {Middleware}Extensions
{
    public static IApplicationBuilder Use{Middleware}(this IApplicationBuilder app)
    {
        return app.UseMiddleware<{Middleware}Middleware>();
    }
}
```

### 3. 创建单元测试

```csharp
namespace Dawning.{Package}.Tests;

public class {Feature}Tests
{
    [Fact]
    public void {MethodName}_{Scenario}_{ExpectedResult}()
    {
        // Arrange
        var input = "test";
        
        // Act
        var result = input.{MethodName}();
        
        // Assert
        Assert.Equal("expected", result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("hello", "Hello")]
    public void {MethodName}_VariousInputs_ReturnsExpected(string? input, string expected)
    {
        var result = input?.{MethodName}() ?? "";
        Assert.Equal(expected, result);
    }
}
```

### 4. 更新 README

```markdown
## {Feature} 功能

### 安装

​```bash
dotnet add package Dawning.{Package}
​```

### 使用方法

​```csharp
// 使用示例
var result = input.{MethodName}();
​```

### API 参考

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `{MethodName}()` | {描述} | {类型} |
```

## 代码规范

### 必须遵循
- ✅ 使用 file-scoped namespaces
- ✅ 添加完整的 XML 文档注释
- ✅ 包含 `<example>` 代码示例
- ✅ 处理 null 参数
- ✅ 单元测试覆盖率 > 80%
- ✅ 遵循方法命名规范

### 测试命名规范
```
{MethodName}_{StateUnderTest}_{ExpectedBehavior}

例如：
Truncate_StringLongerThanMax_ReturnsTruncatedWithSuffix
IsNullOrEmpty_NullInput_ReturnsTrue
```

## 发布准备

1. 确保所有测试通过
2. 更新 CHANGELOG.md
3. 在 Directory.Build.props 中更新版本号（如需要）
4. 创建 PR 并通过代码审查
