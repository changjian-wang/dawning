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
        private readonly IServiceScopeFactory _serviceScopeFactory;

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
            ILogger<RequestLoggingMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
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
                // 使用 Fire-and-forget 模式记录到数据库，不阻塞响应
                // 捕获必要的值避免闭包问题
                var logContext = new LogContext
                {
                    RequestId = requestId,
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value ?? "/",
                    QueryString = context.Request.QueryString.Value,
                    StatusCode = context.Response.StatusCode,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ClientIp = GetClientIp(context),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    UserId = GetUserId(context),
                    UserName = GetUserName(context),
                    RequestTime = requestTime,
                    Exception = exception,
                };

                // Fire-and-forget，使用应用级别的 IServiceScopeFactory 不阻塞响应
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var loggingService =
                            scope.ServiceProvider.GetService<IRequestLoggingService>();
                        if (loggingService == null)
                            return;

                        var entry = new RequestLogEntry
                        {
                            RequestId = logContext.RequestId,
                            Method = logContext.Method,
                            Path = logContext.Path,
                            QueryString = logContext.QueryString,
                            StatusCode = logContext.StatusCode,
                            ResponseTimeMs = logContext.ResponseTimeMs,
                            ClientIp = logContext.ClientIp,
                            UserAgent = logContext.UserAgent,
                            UserId = logContext.UserId,
                            UserName = logContext.UserName,
                            RequestTime = logContext.RequestTime,
                            Exception = logContext.Exception,
                        };

                        await loggingService.LogRequestAsync(entry);
                    }
                    catch (Exception ex)
                    {
                        // 日志写入失败不应影响主流程，只记录到文件日志
                        _logger.LogError(ex, "Failed to log request to database (async)");
                    }
                });
            }
        }

        private Guid? GetUserId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return null;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
        }

        private string? GetUserName(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return null;
            return context.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// 日志上下文，用于异步记录
        /// </summary>
        private class LogContext
        {
            public string RequestId { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public string? QueryString { get; set; }
            public int StatusCode { get; set; }
            public long ResponseTimeMs { get; set; }
            public string? ClientIp { get; set; }
            public string? UserAgent { get; set; }
            public Guid? UserId { get; set; }
            public string? UserName { get; set; }
            public DateTime RequestTime { get; set; }
            public string? Exception { get; set; }
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
