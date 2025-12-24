using System.Net;

namespace Dawning.Core.Exceptions;

/// <summary>
/// 业务异常基类 - 用于可预期的业务错误
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// 额外数据
    /// </summary>
    public new object? Data { get; }

    public BusinessException(string message, string errorCode = "BUSINESS_ERROR", HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? data = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Data = data;
    }
}

/// <summary>
/// 资源未找到异常
/// </summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string resource, object? id = null)
        : base(
            id != null ? $"{resource} with id '{id}' was not found" : $"{resource} was not found",
            "NOT_FOUND",
            HttpStatusCode.NotFound)
    { }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND", HttpStatusCode.NotFound)
    { }
}

/// <summary>
/// 未授权异常
/// </summary>
public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "UNAUTHORIZED", HttpStatusCode.Unauthorized)
    { }
}

/// <summary>
/// 禁止访问异常
/// </summary>
public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, "FORBIDDEN", HttpStatusCode.Forbidden)
    { }

    public ForbiddenException(string resource, string action)
        : base($"You don't have permission to {action} {resource}", "FORBIDDEN", HttpStatusCode.Forbidden)
    { }
}

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : BusinessException
{
    /// <summary>
    /// 验证错误详情
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]>? errors = null)
        : base(message, "VALIDATION_ERROR", HttpStatusCode.BadRequest)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error)
        : base($"Validation failed for field '{field}'", "VALIDATION_ERROR", HttpStatusCode.BadRequest)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}

/// <summary>
/// 冲突异常 (如重复数据)
/// </summary>
public class ConflictException : BusinessException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", HttpStatusCode.Conflict)
    { }

    public ConflictException(string resource, string field, object value)
        : base($"{resource} with {field} '{value}' already exists", "CONFLICT", HttpStatusCode.Conflict)
    { }
}

/// <summary>
/// 请求过多异常 (限流)
/// </summary>
public class TooManyRequestsException : BusinessException
{
    /// <summary>
    /// 重试等待时间 (秒)
    /// </summary>
    public int? RetryAfterSeconds { get; }

    public TooManyRequestsException(string message = "Too many requests", int? retryAfterSeconds = null)
        : base(message, "TOO_MANY_REQUESTS", HttpStatusCode.TooManyRequests)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// 服务不可用异常
/// </summary>
public class ServiceUnavailableException : BusinessException
{
    public ServiceUnavailableException(string service, string? reason = null)
        : base(
            reason != null ? $"Service '{service}' is unavailable: {reason}" : $"Service '{service}' is unavailable",
            "SERVICE_UNAVAILABLE",
            HttpStatusCode.ServiceUnavailable)
    { }
}
