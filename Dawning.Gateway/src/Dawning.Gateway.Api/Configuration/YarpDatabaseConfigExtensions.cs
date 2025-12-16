using Yarp.ReverseProxy.Configuration;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// YARP 数据库配置扩展方法
/// </summary>
public static class YarpDatabaseConfigExtensions
{
    /// <summary>
    /// 添加数据库配置提供程序
    /// </summary>
    public static IReverseProxyBuilder LoadFromDatabase(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IGatewayConfigService, GatewayConfigService>();
        builder.Services.AddSingleton<DatabaseProxyConfigProvider>();
        builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
            sp.GetRequiredService<DatabaseProxyConfigProvider>()
        );

        return builder;
    }

    /// <summary>
    /// 初始化数据库配置（在应用启动时调用）
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
    /// 映射配置重载端点
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
                            detail: ex.Message,
                            statusCode: 500,
                            title: "Failed to reload configuration"
                        );
                    }
                }
            )
            .RequireAuthorization("admin")
            .WithName("ReloadGatewayConfig")
            .WithTags("Gateway");

        return endpoints;
    }
}
