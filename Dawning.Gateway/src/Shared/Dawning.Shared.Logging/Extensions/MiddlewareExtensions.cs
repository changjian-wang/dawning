using Dawning.Shared.Logging.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Dawning.Shared.Logging.Extensions;

/// <summary>
/// 中间件扩展
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// 使用日志上下文富化中间件
    /// 自动将请求信息添加到日志上下文中
    /// </summary>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// app.UseDawningLoggingEnrichment();
    /// // ...
    /// </code>
    /// </example>
    public static IApplicationBuilder UseDawningLoggingEnrichment(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LogEnrichmentMiddleware>();
    }
}
