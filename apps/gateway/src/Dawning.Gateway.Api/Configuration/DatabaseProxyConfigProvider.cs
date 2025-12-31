using Dawning.Identity.Application.Interfaces;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// YARP configuration provider - loads route and cluster configuration from database
/// </summary>
public class DatabaseProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly IGatewayConfigService _configService;
    private readonly ILogger<DatabaseProxyConfigProvider> _logger;
    private volatile DatabaseProxyConfig _config;
    private CancellationTokenSource _changeTokenSource;
    private readonly object _lock = new();

    public DatabaseProxyConfigProvider(
        IGatewayConfigService configService,
        ILogger<DatabaseProxyConfigProvider> logger
    )
    {
        _configService = configService;
        _logger = logger;
        _changeTokenSource = new CancellationTokenSource();
        _config = new DatabaseProxyConfig(
            Array.Empty<RouteConfig>(),
            Array.Empty<ClusterConfig>(),
            new CancellationChangeToken(_changeTokenSource.Token)
        );
    }

    public IProxyConfig GetConfig() => _config;

    /// <summary>
    /// Load configuration from database
    /// </summary>
    public async Task LoadConfigAsync()
    {
        try
        {
            _logger.LogInformation("Loading YARP configuration from database...");

            var routes = await _configService.GetRoutesAsync();
            var clusters = await _configService.GetClustersAsync();

            _logger.LogInformation(
                "Loaded {RouteCount} routes and {ClusterCount} clusters from database",
                routes.Count,
                clusters.Count
            );

            UpdateConfig(routes, clusters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load YARP configuration from database");
            throw;
        }
    }

    /// <summary>
    /// Reload configuration (triggered after configuration update)
    /// </summary>
    public async Task ReloadConfigAsync()
    {
        await LoadConfigAsync();
        _logger.LogInformation("YARP configuration reloaded successfully");
    }

    private void UpdateConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters
    )
    {
        lock (_lock)
        {
            var oldTokenSource = _changeTokenSource;
            _changeTokenSource = new CancellationTokenSource();

            _config = new DatabaseProxyConfig(
                routes,
                clusters,
                new CancellationChangeToken(_changeTokenSource.Token)
            );

            // Trigger configuration change
            oldTokenSource.Cancel();
            oldTokenSource.Dispose();
        }
    }

    public void Dispose()
    {
        _changeTokenSource?.Cancel();
        _changeTokenSource?.Dispose();
    }
}

/// <summary>
/// Database configuration snapshot
/// </summary>
public class DatabaseProxyConfig : IProxyConfig
{
    public DatabaseProxyConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        IChangeToken changeToken
    )
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = changeToken;
    }

    public IReadOnlyList<RouteConfig> Routes { get; }
    public IReadOnlyList<ClusterConfig> Clusters { get; }
    public IChangeToken ChangeToken { get; }
}
