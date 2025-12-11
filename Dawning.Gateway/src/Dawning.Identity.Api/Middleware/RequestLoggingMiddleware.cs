using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// 请求日志中间件
    /// 记录所有HTTP请求的详细信息
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N");
            
            // 记录请求信息
            var requestInfo = await FormatRequest(context.Request, requestId);
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
                var responseInfo = $"[Response {requestId}] Status: {context.Response.StatusCode}, Time: {stopwatch.ElapsedMilliseconds}ms";
                
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
                _logger.LogError(ex, $"Request {requestId} failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                throw;
            }
        }

        private async Task<string> FormatRequest(HttpRequest request, string requestId)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[Request {requestId}]");
            builder.AppendLine($"  Method: {request.Method}");
            builder.AppendLine($"  Path: {request.Path}{request.QueryString}");
            builder.AppendLine($"  Scheme: {request.Scheme}");
            builder.AppendLine($"  Host: {request.Host}");
            builder.AppendLine($"  ClientIP: {request.HttpContext.Connection.RemoteIpAddress}");
            builder.AppendLine($"  UserAgent: {request.Headers["User-Agent"]}");
            builder.AppendLine($"  ContentType: {request.ContentType}");
            builder.AppendLine($"  ContentLength: {request.ContentLength}");

            // 记录请求头（排除敏感信息）
            builder.AppendLine("  Headers:");
            foreach (var header in request.Headers)
            {
                if (!IsSensitiveHeader(header.Key))
                {
                    builder.AppendLine($"    {header.Key}: {header.Value}");
                }
            }

            return builder.ToString();
        }

        private async Task<string> FormatResponse(HttpResponse response, string requestId, long elapsedMs)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[Response {requestId}]");
            builder.AppendLine($"  StatusCode: {response.StatusCode}");
            builder.AppendLine($"  ContentType: {response.ContentType}");
            builder.AppendLine($"  ContentLength: {response.ContentLength}");
            builder.AppendLine($"  ElapsedTime: {elapsedMs}ms");

            return builder.ToString();
        }

        private bool IsSensitiveHeader(string headerName)
        {
            var sensitiveHeaders = new[] { "Authorization", "Cookie", "Set-Cookie", "X-API-Key" };
            return Array.Exists(sensitiveHeaders, h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
