using System.Net;

namespace Dawning.Core.Exceptions;

/// <summary>
/// Business exception base class - for expected business errors
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Additional data
    /// </summary>
    public new object? Data { get; }

    public BusinessException(
        string message,
        string errorCode = "BUSINESS_ERROR",
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        object? data = null
    )
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Data = data;
    }
}

/// <summary>
/// Resource not found exception
/// </summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string resource, object? id = null)
        : base(
            id != null ? $"{resource} with id '{id}' was not found" : $"{resource} was not found",
            "NOT_FOUND",
            HttpStatusCode.NotFound
        ) { }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND", HttpStatusCode.NotFound) { }
}

/// <summary>
/// Unauthorized access exception
/// </summary>
public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "UNAUTHORIZED", HttpStatusCode.Unauthorized) { }
}

/// <summary>
/// Access forbidden exception
/// </summary>
public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, "FORBIDDEN", HttpStatusCode.Forbidden) { }

    public ForbiddenException(string resource, string action)
        : base(
            $"You don't have permission to {action} {resource}",
            "FORBIDDEN",
            HttpStatusCode.Forbidden
        ) { }
}

/// <summary>
/// Validation exception
/// </summary>
public class ValidationException : BusinessException
{
    /// <summary>
    /// Validation error details
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]>? errors = null)
        : base(message, "VALIDATION_ERROR", HttpStatusCode.BadRequest)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error)
        : base(
            $"Validation failed for field '{field}'",
            "VALIDATION_ERROR",
            HttpStatusCode.BadRequest
        )
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

/// <summary>
/// Conflict exception (e.g., duplicate data)
/// </summary>
public class ConflictException : BusinessException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", HttpStatusCode.Conflict) { }

    public ConflictException(string resource, string field, object value)
        : base(
            $"{resource} with {field} '{value}' already exists",
            "CONFLICT",
            HttpStatusCode.Conflict
        ) { }
}

/// <summary>
/// Too many requests exception (rate limiting)
/// </summary>
public class TooManyRequestsException : BusinessException
{
    /// <summary>
    /// Retry wait time in seconds
    /// </summary>
    public int? RetryAfterSeconds { get; }

    public TooManyRequestsException(
        string message = "Too many requests",
        int? retryAfterSeconds = null
    )
        : base(message, "TOO_MANY_REQUESTS", HttpStatusCode.TooManyRequests)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// Service unavailable exception
/// </summary>
public class ServiceUnavailableException : BusinessException
{
    public ServiceUnavailableException(string service, string? reason = null)
        : base(
            reason != null
                ? $"Service '{service}' is unavailable: {reason}"
                : $"Service '{service}' is unavailable",
            "SERVICE_UNAVAILABLE",
            HttpStatusCode.ServiceUnavailable
        ) { }
}
