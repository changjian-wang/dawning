using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Dawning.Logging.Middleware;

/// <summary>
/// Log context enrichment middleware
/// Automatically adds request-related log properties
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
        // Extract request-related information
        var traceId = context.TraceIdentifier;
        var userId =
            context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst("user_id")?.Value
            ?? "anonymous";
        var userName =
            context.User?.FindFirst("name")?.Value
            ?? context.User?.FindFirst("preferred_username")?.Value;
        var clientIp = GetClientIp(context);
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        var tenantId = context.User?.FindFirst("tenant_id")?.Value;

        // Add properties using Serilog's LogContext
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
    /// Gets the client's real IP address
    /// </summary>
    private static string GetClientIp(HttpContext context)
    {
        // Prefer getting from proxy headers first
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For may contain multiple IPs, take the first one
            return forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
