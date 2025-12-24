using Dawning.Shared.Core.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Dawning.Shared.Core.Extensions;

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// 使用全局异常处理中间件
    /// 应该在中间件管道的最外层使用
    /// </summary>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// app.UseDawningExceptionHandling(); // 放在最前面
    /// app.UseRouting();
    /// // ... 其他中间件
    /// </code>
    /// </example>
    public static IApplicationBuilder UseDawningExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
