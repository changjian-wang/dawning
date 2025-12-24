using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Dawning.Shared.Logging.Middleware;

/// <summary>
/// 日志上下文富化中间件
/// 自动添加请求相关的日志属性
/// </summary>
public class LogEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public LogEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 提取请求相关信息
        var traceId = context.TraceIdentifier;
        var userId = context.User?.FindFirst("sub")?.Value 
                  ?? context.User?.FindFirst("user_id")?.Value 
                  ?? "anonymous";
        var userName = context.User?.FindFirst("name")?.Value 
                    ?? context.User?.FindFirst("preferred_username")?.Value;
        var clientIp = GetClientIp(context);
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        var tenantId = context.User?.FindFirst("tenant_id")?.Value;

        // 使用 Serilog 的 LogContext 添加属性
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserName", userName ?? ""))
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("UserAgent", userAgent ?? ""))
        using (LogContext.PushProperty("TenantId", tenantId ?? ""))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// 获取客户端真实 IP
    /// </summary>
    private static string GetClientIp(HttpContext context)
    {
        // 优先从代理头获取
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For 可能包含多个 IP，取第一个
            return forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 回退到连接 IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
