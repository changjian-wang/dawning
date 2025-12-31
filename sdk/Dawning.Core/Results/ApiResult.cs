using System.Text.Json.Serialization;

namespace Dawning.Core.Results;

/// <summary>
/// Unified API response result
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Status code
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = "OK";

    /// <summary>
    /// Message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Data payload
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// Request trace ID
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}

/// <summary>
/// Unified API response result (without data)
/// </summary>
public class ApiResult : ApiResult<object> { }

/// <summary>
/// API result factory
/// </summary>
public static class ApiResults
{
    /// <summary>
    /// Success response
    /// </summary>
    public static ApiResult<T> Ok<T>(T data, string? message = null)
    {
        return new ApiResult<T>
        {
            Success = true,
            Code = "OK",
            Message = message,
            Data = data,
        };
    }

    /// <summary>
    /// Success response (without data)
    /// </summary>
    public static ApiResult Ok(string? message = null)
    {
        return new ApiResult
        {
            Success = true,
            Code = "OK",
            Message = message,
        };
    }

    /// <summary>
    /// Created successfully
    /// </summary>
    public static ApiResult<T> Created<T>(T data, string? message = null)
    {
        return new ApiResult<T>
        {
            Success = true,
            Code = "CREATED",
            Message = message ?? "Resource created successfully",
            Data = data,
        };
    }

    /// <summary>
    /// Failure response
    /// </summary>
    public static ApiResult<T> Fail<T>(string code, string message, T? data = default)
    {
        return new ApiResult<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Data = data,
        };
    }

    /// <summary>
    /// Failure response (without data)
    /// </summary>
    public static ApiResult Fail(string code, string message)
    {
        return new ApiResult
        {
            Success = false,
            Code = code,
            Message = message,
        };
    }

    /// <summary>
    /// Resource not found
    /// </summary>
    public static ApiResult NotFound(string message = "Resource not found")
    {
        return Fail("NOT_FOUND", message);
    }

    /// <summary>
    /// Validation failed
    /// </summary>
    public static ApiResult ValidationError(
        IDictionary<string, string[]>? errors = null,
        string message = "Validation failed"
    )
    {
        return new ApiResult
        {
            Success = false,
            Code = "VALIDATION_ERROR",
            Message = message,
            Data = errors,
        };
    }

    /// <summary>
    /// Unauthorized access
    /// </summary>
    public static ApiResult Unauthorized(string message = "Unauthorized")
    {
        return Fail("UNAUTHORIZED", message);
    }

    /// <summary>
    /// Access forbidden
    /// </summary>
    public static ApiResult Forbidden(string message = "Access forbidden")
    {
        return Fail("FORBIDDEN", message);
    }

    /// <summary>
    /// Internal server error
    /// </summary>
    public static ApiResult Error(string message = "Internal server error", string? traceId = null)
    {
        return new ApiResult
        {
            Success = false,
            Code = "INTERNAL_ERROR",
            Message = message,
            TraceId = traceId,
        };
    }
}
