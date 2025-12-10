using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// 性能监控中间件
    /// 监控API响应时间和性能指标
    /// </summary>
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private const int SlowRequestThresholdMs = 3000; // 3秒
        private const int VerySlowRequestThresholdMs = 10000; // 10秒

        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // 记录性能指标
                if (elapsedMs > VerySlowRequestThresholdMs)
                {
                    _logger.LogError(
                        "Very slow request detected: {Method} {Path} took {ElapsedMs}ms (Status: {StatusCode})",
                        method, path, elapsedMs, context.Response.StatusCode);
                }
                else if (elapsedMs > SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {ElapsedMs}ms (Status: {StatusCode})",
                        method, path, elapsedMs, context.Response.StatusCode);
                }
                else
                {
                    _logger.LogDebug(
                        "Request completed: {Method} {Path} took {ElapsedMs}ms (Status: {StatusCode})",
                        method, path, elapsedMs, context.Response.StatusCode);
                }

                // 添加性能头到响应
                context.Response.Headers.Append("X-Response-Time-Ms", elapsedMs.ToString());
            }
        }
    }

    /// <summary>
    /// 性能指标收集器
    /// </summary>
    public class PerformanceMetrics
    {
        public static long TotalRequests { get; private set; }
        public static long TotalErrors { get; private set; }
        public static long SlowRequests { get; private set; }
        public static double AverageResponseTime { get; private set; }
        private static long _totalResponseTime;

        public static void RecordRequest(long responseTimeMs, bool isError, bool isSlow)
        {
            TotalRequests++;
            _totalResponseTime += responseTimeMs;
            AverageResponseTime = (double)_totalResponseTime / TotalRequests;

            if (isError) TotalErrors++;
            if (isSlow) SlowRequests++;
        }

        public static void Reset()
        {
            TotalRequests = 0;
            TotalErrors = 0;
            SlowRequests = 0;
            AverageResponseTime = 0;
            _totalResponseTime = 0;
        }
    }
}
