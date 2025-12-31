using System.Text.Json;
using Dawning.Core.Exceptions;
using Dawning.Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dawning.Core.Middleware;

/// <summary>
/// Global exception handling middleware
/// Automatically converts BusinessException to standard API responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, result) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                ApiResults.ValidationError(validationEx.Errors, validationEx.Message)
            ),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                ApiResults.NotFound(notFoundEx.Message)
            ),

            UnauthorizedException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                ApiResults.Unauthorized(unauthorizedEx.Message)
            ),

            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                ApiResults.Forbidden(forbiddenEx.Message)
            ),

            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                ApiResults.Fail(conflictEx.ErrorCode, conflictEx.Message)
            ),

            TooManyRequestsException rateLimitEx => (
                StatusCodes.Status429TooManyRequests,
                CreateRateLimitResult(rateLimitEx)
            ),

            ServiceUnavailableException serviceEx => (
                StatusCodes.Status503ServiceUnavailable,
                ApiResults.Fail(serviceEx.ErrorCode, serviceEx.Message)
            ),

            BusinessException businessEx => (
                StatusCodes.Status400BadRequest,
                ApiResults.Fail(businessEx.ErrorCode, businessEx.Message)
            ),

            OperationCanceledException => (
                499, // Client Closed Request
                ApiResults.Fail("REQUEST_CANCELLED", "Request cancelled")
            ),

            _ => HandleUnexpectedException(exception),
        };

        // Log the exception
        LogException(exception, statusCode, context);

        // Return response
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonOptions));
    }

    private static ApiResult CreateRateLimitResult(TooManyRequestsException ex)
    {
        var result = ApiResults.Fail(ex.ErrorCode, ex.Message);
        // Additional rate limit info can be added to Data here
        return result;
    }

    private (int statusCode, ApiResult result) HandleUnexpectedException(Exception exception)
    {
        // Unexpected exception, return 500
        return (
            StatusCodes.Status500InternalServerError,
            ApiResults.Error("Internal server error, please try again later")
        );
    }

    private void LogException(Exception exception, int statusCode, HttpContext context)
    {
        var requestPath = context.Request.Path;
        var method = context.Request.Method;
        var traceId = context.TraceIdentifier;

        if (exception is BusinessException businessEx)
        {
            // Business exceptions use Warning level
            _logger.LogWarning(
                exception,
                "[{TraceId}] Business exception - {Method} {Path} - Code: {ErrorCode}, Message: {Message}",
                traceId,
                method,
                requestPath,
                businessEx.ErrorCode,
                businessEx.Message
            );
        }
        else if (exception is OperationCanceledException)
        {
            // Request cancellation uses Information level
            _logger.LogInformation(
                "[{TraceId}] Request cancelled - {Method} {Path}",
                traceId,
                method,
                requestPath
            );
        }
        else
        {
            // Unexpected exceptions use Error level
            _logger.LogError(
                exception,
                "[{TraceId}] Unhandled exception - {Method} {Path}",
                traceId,
                method,
                requestPath
            );
        }
    }
}

/// <summary>
/// Extended HTTP status codes (non-standard)
/// </summary>
public static class StatusCodesExtensions
{
    /// <summary>
    /// 499 Client Closed Request (Nginx extension)
    /// </summary>
    public const int Status499ClientClosedRequest = 499;
}
