using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// 请求日志中间件
    /// 记录所有HTTP请求的详细信息到日志文件和数据库
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        // 要忽略的路径前缀
        private static readonly string[] IgnorePaths = new[]
        {
            "/health",
            "/swagger",
            "/favicon.ico",
            "/_framework",
            "/api/monitoring/realtime",
        };

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 跳过某些路径
            if (ShouldIgnore(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var requestId = context.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            var requestTime = DateTime.UtcNow;
            string? exception = null;

            // 记录请求信息到日志文件
            var requestInfo = FormatRequest(context.Request, requestId);
            _logger.LogInformation(requestInfo);

            // 使用 OnStarting 回调安全地添加响应头
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey("X-Request-Id"))
                {
                    context.Response.Headers["X-Request-Id"] = requestId;
                }
                return Task.CompletedTask;
            });

            try
            {
                // 直接执行下一个中间件，不拦截响应体
                await _next(context);

                stopwatch.Stop();

                // 记录响应基本信息（不读取响应体）
                var responseInfo =
                    $"[Response {requestId}] Status: {context.Response.StatusCode}, Time: {stopwatch.ElapsedMilliseconds}ms";

                if (stopwatch.ElapsedMilliseconds > 3000)
                {
                    _logger.LogWarning($"Slow request detected: {responseInfo}");
                }
                else
                {
                    _logger.LogInformation(responseInfo);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                exception = ex.ToString();
                _logger.LogError(
                    ex,
                    $"Request {requestId} failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}"
                );
                throw;
            }
            finally
            {
                // 记录到数据库
                await LogToDatabaseAsync(
                    context,
                    requestId,
                    requestTime,
                    stopwatch.ElapsedMilliseconds,
                    exception
                );
            }
        }

        private async Task LogToDatabaseAsync(
            HttpContext context,
            string requestId,
            DateTime requestTime,
            long responseTimeMs,
            string? exception
        )
        {
            try
            {
                var loggingService = context.RequestServices.GetService<IRequestLoggingService>();
                if (loggingService == null)
                    return;

                // 获取用户信息
                Guid? userId = null;
                string? userName = null;
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdClaim, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                    userName = context.User.FindFirst(ClaimTypes.Name)?.Value;
                }

                var entry = new RequestLogEntry
                {
                    RequestId = requestId,
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value ?? "/",
                    QueryString = context.Request.QueryString.Value,
                    StatusCode = context.Response.StatusCode,
                    ResponseTimeMs = responseTimeMs,
                    ClientIp = GetClientIp(context),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    UserId = userId,
                    UserName = userName,
                    RequestTime = requestTime,
                    RequestBodySize = context.Request.ContentLength,
                    ResponseBodySize = context.Response.ContentLength,
                    Exception = exception,
                };

                // 异步写入数据库（fire and forget）
                _ = Task.Run(() => loggingService.LogRequestAsync(entry));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log request to database");
            }
        }

        private static string? GetClientIp(HttpContext context)
        {
            // Try to get real IP from X-Forwarded-For header (for reverse proxy)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            // Try X-Real-IP header
            var realIp = context.Request.Headers["X-Real-IP"].ToString();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString();
        }

        private static bool ShouldIgnore(PathString path)
        {
            if (!path.HasValue)
                return false;

            foreach (var ignorePath in IgnorePaths)
            {
                if (path.Value!.StartsWith(ignorePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string FormatRequest(HttpRequest request, string requestId)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[Request {requestId}]");
            builder.AppendLine($"  Method: {request.Method}");
            builder.AppendLine($"  Path: {request.Path}{request.QueryString}");
            builder.AppendLine($"  Scheme: {request.Scheme}");
            builder.AppendLine($"  Host: {request.Host}");
            builder.AppendLine($"  ClientIP: {GetClientIp(request.HttpContext)}");
            builder.AppendLine($"  UserAgent: {request.Headers["User-Agent"]}");
            builder.AppendLine($"  ContentType: {request.ContentType}");
            builder.AppendLine($"  ContentLength: {request.ContentLength}");

            return builder.ToString();
        }

        private static bool IsSensitiveHeader(string headerName)
        {
            var sensitiveHeaders = new[] { "Authorization", "Cookie", "Set-Cookie", "X-API-Key" };
            return Array.Exists(
                sensitiveHeaders,
                h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}
