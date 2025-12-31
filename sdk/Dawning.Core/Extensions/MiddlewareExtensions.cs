using Dawning.Core.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Dawning.Core.Extensions;

/// <summary>
/// Middleware extension methods
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Use global exception handling middleware
    /// Should be used at the outermost layer of the middleware pipeline
    /// </summary>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// app.UseDawningExceptionHandling(); // Place at the beginning
    /// app.UseRouting();
    /// // ... other middleware
    /// </code>
    /// </example>
    public static IApplicationBuilder UseDawningExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
