using Dawning.Logging.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Dawning.Logging.Extensions;

/// <summary>
/// Middleware extensions
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Uses the log context enrichment middleware
    /// Automatically adds request information to the log context
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
