using Yarp.ReverseProxy.Configuration;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// YARP database configuration extension methods
/// </summary>
public static class YarpDatabaseConfigExtensions
{
    private static readonly SemaphoreSlim ReloadGate = new(1, 1);

    /// <summary>
    /// Add database configuration provider
    /// </summary>
    public static IReverseProxyBuilder LoadFromDatabase(this IReverseProxyBuilder builder)
    {
        // Register configuration provider
        // Note: GatewayConfigService and Repository are auto-registered by NativeInjectorBootStrapper
        builder.Services.AddSingleton<DatabaseProxyConfigProvider>();
        builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
            sp.GetRequiredService<DatabaseProxyConfigProvider>()
        );

        return builder;
    }

    /// <summary>
    /// Initialize database configuration (called at application startup)
    /// </summary>
    public static async Task<IApplicationBuilder> InitializeDatabaseProxyConfigAsync(
        this IApplicationBuilder app
    )
    {
        var configProvider =
            app.ApplicationServices.GetRequiredService<DatabaseProxyConfigProvider>();
        await configProvider.LoadConfigAsync();
        return app;
    }

    /// <summary>
    /// Map configuration reload endpoint
    /// </summary>
    public static IEndpointRouteBuilder MapProxyConfigReloadEndpoint(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/gateway/reload"
    )
    {
        endpoints
            .MapPost(
                pattern,
                async (
                    DatabaseProxyConfigProvider configProvider,
                    ILogger<DatabaseProxyConfigProvider> logger
                ) =>
                {
                    if (!await ReloadGate.WaitAsync(0))
                    {
                        logger.LogWarning("Gateway configuration reload already in progress");
                        return Results.Problem(
                            detail: "A configuration reload is already in progress. Please retry later.",
                            statusCode: 409,
                            title: "Reload already in progress"
                        );
                    }

                    try
                    {
                        await configProvider.ReloadConfigAsync();
                        logger.LogInformation("Gateway configuration reloaded via API");
                        return Results.Ok(
                            new { success = true, message = "Configuration reloaded successfully" }
                        );
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to reload gateway configuration");
                        return Results.Problem(
                            detail: "An internal error occurred while reloading the configuration.",
                            statusCode: 500,
                            title: "Failed to reload configuration"
                        );
                    }
                    finally
                    {
                        ReloadGate.Release();
                    }
                }
            )
            .RequireAuthorization("admin")
            .WithName("ReloadGatewayConfig")
            .WithTags("Gateway");

        return endpoints;
    }
}
