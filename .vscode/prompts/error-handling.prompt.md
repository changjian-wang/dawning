---
description: 异常处理和错误码规范
---

# 异常处理规范

统一的异常处理和错误响应规范。

## 异常类型体系

```
Dawning.Core.Exceptions/
├── DawningException.cs          # 基类
├── BusinessException.cs         # 业务异常（400）
├── NotFoundException.cs         # 资源不存在（404）
├── UnauthorizedException.cs     # 未认证（401）
├── ForbiddenException.cs        # 无权限（403）
├── ValidationException.cs       # 验证失败（400）
└── ConflictException.cs         # 资源冲突（409）
```

## 异常类定义

### 基础异常类

```csharp
namespace Dawning.Core.Exceptions;

/// <summary>
/// Dawning 异常基类
/// </summary>
public abstract class DawningException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public string Code { get; }
    
    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public int StatusCode { get; }

    protected DawningException(string code, string message, int statusCode = 400) 
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
```

### 具体异常类

```csharp
/// <summary>
/// 业务异常（HTTP 400）
/// </summary>
public class BusinessException : DawningException
{
    public BusinessException(string message, string code = "BUSINESS_ERROR")
        : base(code, message, 400) { }
}

/// <summary>
/// 资源未找到异常（HTTP 404）
/// </summary>
public class NotFoundException : DawningException
{
    public NotFoundException(string message, string code = "NOT_FOUND")
        : base(code, message, 404) { }
    
    public static NotFoundException ForEntity<T>(Guid id) =>
        new($"{typeof(T).Name} with id '{id}' not found", $"{typeof(T).Name.ToUpper()}_NOT_FOUND");
}

/// <summary>
/// 未认证异常（HTTP 401）
/// </summary>
public class UnauthorizedException : DawningException
{
    public UnauthorizedException(string message = "Authentication required", string code = "UNAUTHORIZED")
        : base(code, message, 401) { }
}

/// <summary>
/// 禁止访问异常（HTTP 403）
/// </summary>
public class ForbiddenException : DawningException
{
    public ForbiddenException(string message = "Access denied", string code = "FORBIDDEN")
        : base(code, message, 403) { }
}

/// <summary>
/// 验证失败异常（HTTP 400）
/// </summary>
public class ValidationException : DawningException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base("VALIDATION_FAILED", message, 400)
    {
        Errors = errors ?? new();
    }
}

/// <summary>
/// 资源冲突异常（HTTP 409）
/// </summary>
public class ConflictException : DawningException
{
    public ConflictException(string message, string code = "CONFLICT")
        : base(code, message, 409) { }
}
```

## 全局异常处理中间件

```csharp
namespace Dawning.Core.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DawningException ex)
        {
            logger.LogWarning(ex, "Business exception: {Code} - {Message}", ex.Code, ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, 500, "INTERNAL_ERROR", "An unexpected error occurred");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, 
        int statusCode, 
        string code, 
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var response = ApiResult<object>.Error(code, message);
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## 在 Service 中使用

```csharp
public class UserService(IUnitOfWork unitOfWork) : IUserService
{
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await unitOfWork.User.GetByIdAsync(id);
        
        // ✅ 使用具体异常类型
        if (user == null)
            throw NotFoundException.ForEntity<User>(id);
            
        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        // ✅ 业务规则验证
        if (await unitOfWork.User.ExistsByUsernameAsync(dto.Username))
            throw new ConflictException($"Username '{dto.Username}' already exists", "USERNAME_EXISTS");
            
        if (await unitOfWork.User.ExistsByEmailAsync(dto.Email))
            throw new ConflictException($"Email '{dto.Email}' already exists", "EMAIL_EXISTS");
        
        var user = dto.ToEntity();
        await unitOfWork.User.InsertAsync(user);
        return user.ToDto();
    }

    public async Task UpdatePasswordAsync(Guid id, ChangePasswordDto dto)
    {
        var user = await unitOfWork.User.GetByIdAsync(id)
            ?? throw NotFoundException.ForEntity<User>(id);
            
        // ✅ 验证旧密码
        if (!PasswordHasher.Verify(dto.OldPassword, user.PasswordHash))
            throw new BusinessException("Current password is incorrect", "INVALID_PASSWORD");
            
        // ✅ 验证新密码强度
        if (dto.NewPassword.Length < 8)
            throw new ValidationException("Password must be at least 8 characters", new Dictionary<string, string[]>
            {
                ["newPassword"] = ["Password must be at least 8 characters"]
            });
            
        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        await unitOfWork.User.UpdateAsync(user);
    }
}
```

## 错误码规范

| 错误码前缀 | 说明 | 示例 |
|-----------|------|------|
| `AUTH_` | 认证相关 | `AUTH_EXPIRED`, `AUTH_INVALID_TOKEN` |
| `USER_` | 用户相关 | `USER_NOT_FOUND`, `USER_DISABLED` |
| `ROLE_` | 角色相关 | `ROLE_NOT_FOUND`, `ROLE_IN_USE` |
| `VALIDATION_` | 验证相关 | `VALIDATION_FAILED` |
| `PERMISSION_` | 权限相关 | `PERMISSION_DENIED` |
| `GATEWAY_` | 网关相关 | `GATEWAY_ROUTE_NOT_FOUND` |

## API 错误响应格式

```json
{
  "success": false,
  "code": "USER_NOT_FOUND",
  "message": "User with id '123e4567-e89b-12d3-a456-426614174000' not found",
  "data": null,
  "timestamp": 1704067200000,
  "traceId": "00-abc123-def456-00"
}
```

## 最佳实践

| 实践 | 说明 |
|------|------|
| ✅ 使用具体异常类型 | 不要直接抛出 `Exception` |
| ✅ 提供有意义的错误码 | 便于前端判断和国际化 |
| ✅ 提供清晰的错误消息 | 帮助用户理解问题 |
| ✅ 记录异常日志 | 业务异常用 Warning，系统异常用 Error |
| ❌ 不要暴露敏感信息 | 如堆栈信息、SQL 语句等 |
| ❌ 不要吞掉异常 | 空 catch 块是反模式 |
