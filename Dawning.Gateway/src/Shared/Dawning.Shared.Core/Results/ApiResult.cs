using System.Text.Json.Serialization;

namespace Dawning.Shared.Core.Results;

/// <summary>
/// 统一 API 响应结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = "OK";

    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 请求追踪ID
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}

/// <summary>
/// 统一 API 响应结果 (无数据)
/// </summary>
public class ApiResult : ApiResult<object>
{
}

/// <summary>
/// API 结果工厂
/// </summary>
public static class ApiResults
{
    /// <summary>
    /// 成功响应
    /// </summary>
    public static ApiResult<T> Ok<T>(T data, string? message = null)
    {
        return new ApiResult<T>
        {
            Success = true,
            Code = "OK",
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 成功响应 (无数据)
    /// </summary>
    public static ApiResult Ok(string? message = null)
    {
        return new ApiResult
        {
            Success = true,
            Code = "OK",
            Message = message
        };
    }

    /// <summary>
    /// 创建成功
    /// </summary>
    public static ApiResult<T> Created<T>(T data, string? message = null)
    {
        return new ApiResult<T>
        {
            Success = true,
            Code = "CREATED",
            Message = message ?? "Resource created successfully",
            Data = data
        };
    }

    /// <summary>
    /// 失败响应
    /// </summary>
    public static ApiResult<T> Fail<T>(string code, string message, T? data = default)
    {
        return new ApiResult<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 失败响应 (无数据)
    /// </summary>
    public static ApiResult Fail(string code, string message)
    {
        return new ApiResult
        {
            Success = false,
            Code = code,
            Message = message
        };
    }

    /// <summary>
    /// 资源未找到
    /// </summary>
    public static ApiResult NotFound(string message = "Resource not found")
    {
        return Fail("NOT_FOUND", message);
    }

    /// <summary>
    /// 验证失败
    /// </summary>
    public static ApiResult ValidationError(
        IDictionary<string, string[]>? errors = null,
        string message = "Validation failed")
    {
        return new ApiResult
        {
            Success = false,
            Code = "VALIDATION_ERROR",
            Message = message,
            Data = errors
        };
    }

    /// <summary>
    /// 未授权
    /// </summary>
    public static ApiResult Unauthorized(string message = "Unauthorized")
    {
        return Fail("UNAUTHORIZED", message);
    }

    /// <summary>
    /// 禁止访问
    /// </summary>
    public static ApiResult Forbidden(string message = "Access forbidden")
    {
        return Fail("FORBIDDEN", message);
    }

    /// <summary>
    /// 服务器错误
    /// </summary>
    public static ApiResult Error(string message = "Internal server error", string? traceId = null)
    {
        return new ApiResult
        {
            Success = false,
            Code = "INTERNAL_ERROR",
            Message = message,
            TraceId = traceId
        };
    }
}
