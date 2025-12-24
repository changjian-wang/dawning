using System.Text.Json;
using Dawning.Shared.Core.Exceptions;
using Dawning.Shared.Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dawning.Shared.Core.Middleware;

/// <summary>
/// 全局异常处理中间件
/// 自动将 BusinessException 转换为标准 API 响应
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
                ApiResults.Fail("REQUEST_CANCELLED", "请求已取消")
            ),

            _ => HandleUnexpectedException(exception),
        };

        // 记录日志
        LogException(exception, statusCode, context);

        // 返回响应
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonOptions));
    }

    private static ApiResult CreateRateLimitResult(TooManyRequestsException ex)
    {
        var result = ApiResults.Fail(ex.ErrorCode, ex.Message);
        // 可以在这里添加额外的限流信息到 Data
        return result;
    }

    private (int statusCode, ApiResult result) HandleUnexpectedException(Exception exception)
    {
        // 未预期的异常，返回 500
        return (
            StatusCodes.Status500InternalServerError,
            ApiResults.Error("服务器内部错误，请稍后重试")
        );
    }

    private void LogException(Exception exception, int statusCode, HttpContext context)
    {
        var requestPath = context.Request.Path;
        var method = context.Request.Method;
        var traceId = context.TraceIdentifier;

        if (exception is BusinessException businessEx)
        {
            // 业务异常使用 Warning 级别
            _logger.LogWarning(
                exception,
                "[{TraceId}] 业务异常 - {Method} {Path} - Code: {ErrorCode}, Message: {Message}",
                traceId,
                method,
                requestPath,
                businessEx.ErrorCode,
                businessEx.Message
            );
        }
        else if (exception is OperationCanceledException)
        {
            // 请求取消使用 Information 级别
            _logger.LogInformation(
                "[{TraceId}] 请求取消 - {Method} {Path}",
                traceId,
                method,
                requestPath
            );
        }
        else
        {
            // 未预期异常使用 Error 级别
            _logger.LogError(
                exception,
                "[{TraceId}] 未处理异常 - {Method} {Path}",
                traceId,
                method,
                requestPath
            );
        }
    }
}

/// <summary>
/// 扩展 HTTP 状态码 (非标准)
/// </summary>
public static class StatusCodesExtensions
{
    /// <summary>
    /// 499 Client Closed Request (Nginx 扩展)
    /// </summary>
    public const int Status499ClientClosedRequest = 499;
}
