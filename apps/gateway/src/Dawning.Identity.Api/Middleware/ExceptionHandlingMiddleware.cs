using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// Global Exception Handling Middleware
    /// Captures and logs all unhandled exceptions
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred. Path: {Path}, Method: {Method}, User: {User}",
                    context.Request.Path,
                    context.Request.Method,
                    context.User?.Identity?.Name ?? "Anonymous"
                );

                // Log exception to database
                await LogExceptionToDatabaseAsync(context, ex);

                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Log exception to database
        /// </summary>
        private async Task LogExceptionToDatabaseAsync(HttpContext context, Exception exception)
        {
            try
            {
                // Get ISystemLogService from HttpContext
                var systemLogService = context.RequestServices.GetService<ISystemLogService>();
                if (systemLogService != null)
                {
                // Determine status code
                    int statusCode = exception switch
                    {
                        ArgumentNullException => (int)HttpStatusCode.BadRequest,
                        ArgumentException => (int)HttpStatusCode.BadRequest,
                        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                        InvalidOperationException => (int)HttpStatusCode.Conflict,
                        _ => (int)HttpStatusCode.InternalServerError,
                    };

                    await systemLogService.LogErrorAsync(exception, context, statusCode);
                }
            }
            catch (Exception logEx)
            {
                // Logging failure should not affect the exception handling flow
                _logger.LogError(logEx, "Failed to log exception to database");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Check if response has already started
            if (context.Response.HasStarted)
            {
                // Response has already started, cannot modify status code and headers, return directly
                return;
            }

            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Success = false,
                Message = "An internal server error occurred.",
                ErrorDetails = exception.Message,
                Timestamp = DateTime.UtcNow,
            };

            // Set different status codes based on exception type
            switch (exception)
            {
                case ArgumentNullException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Code = 40000;
                    response.Message = "Invalid request parameters.";
                    break;

                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Code = 40000;
                    response.Message = "Invalid request parameters.";
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Code = 40100;
                    response.Message = "Unauthorized access.";
                    break;

                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Code = 40900;
                    response.Message = "Operation conflict.";
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Code = 50000;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            await context.Response.WriteAsync(jsonResponse);
        }

        private class ErrorResponse
        {
            public bool Success { get; set; }
            public int Code { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? ErrorDetails { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
