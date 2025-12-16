using System.Text.Json;
using Dapper;
using MySqlConnector;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.LoadBalancing;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// 网关配置服务 - 从数据库读取 YARP 配置
/// </summary>
public class GatewayConfigService : IGatewayConfigService
{
    private readonly string _connectionString;
    private readonly ILogger<GatewayConfigService> _logger;

    public GatewayConfigService(IConfiguration configuration, ILogger<GatewayConfigService> logger)
    {
        _connectionString =
            configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("MySQL connection string is not configured");
        _logger = logger;
    }

    public async Task<IReadOnlyList<RouteConfig>> GetRoutesAsync()
    {
        const string sql =
            @"
            SELECT 
                route_id AS RouteId,
                name AS Name,
                cluster_id AS ClusterId,
                match_path AS MatchPath,
                match_methods AS MatchMethods,
                match_hosts AS MatchHosts,
                match_headers AS MatchHeaders,
                match_query_parameters AS MatchQueryParameters,
                authorization_policy AS AuthorizationPolicy,
                rate_limiter_policy AS RateLimiterPolicy,
                cors_policy AS CorsPolicy,
                timeout_seconds AS TimeoutSeconds,
                `order` AS `Order`,
                transform_path_prefix AS TransformPathPrefix,
                transform_path_remove_prefix AS TransformPathRemovePrefix,
                transform_request_headers AS TransformRequestHeaders,
                transform_response_headers AS TransformResponseHeaders,
                metadata AS Metadata
            FROM gateway_routes
            WHERE is_enabled = 1
            ORDER BY `order` ASC, route_id ASC";

        await using var connection = new MySqlConnection(_connectionString);
        var dbRoutes = await connection.QueryAsync<GatewayRouteRecord>(sql);

        var routes = new List<RouteConfig>();
        foreach (var dbRoute in dbRoutes)
        {
            try
            {
                var route = ConvertToRouteConfig(dbRoute);
                routes.Add(route);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert route {RouteId}", dbRoute.RouteId);
            }
        }

        return routes;
    }

    public async Task<IReadOnlyList<ClusterConfig>> GetClustersAsync()
    {
        const string sql =
            @"
            SELECT 
                cluster_id AS ClusterId,
                name AS Name,
                load_balancing_policy AS LoadBalancingPolicy,
                destinations AS Destinations,
                health_check_enabled AS HealthCheckEnabled,
                health_check_interval AS HealthCheckInterval,
                health_check_timeout AS HealthCheckTimeout,
                health_check_path AS HealthCheckPath,
                session_affinity_enabled AS SessionAffinityEnabled,
                session_affinity_policy AS SessionAffinityPolicy,
                session_affinity_key_name AS SessionAffinityKeyName,
                max_connections_per_server AS MaxConnectionsPerServer,
                request_timeout_seconds AS RequestTimeoutSeconds,
                metadata AS Metadata
            FROM gateway_clusters
            WHERE is_enabled = 1";

        await using var connection = new MySqlConnection(_connectionString);
        var dbClusters = await connection.QueryAsync<GatewayClusterRecord>(sql);

        var clusters = new List<ClusterConfig>();
        foreach (var dbCluster in dbClusters)
        {
            try
            {
                var cluster = ConvertToClusterConfig(dbCluster);
                clusters.Add(cluster);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to convert cluster {ClusterId}",
                    dbCluster.ClusterId
                );
            }
        }

        return clusters;
    }

    private RouteConfig ConvertToRouteConfig(GatewayRouteRecord dbRoute)
    {
        // 解析 Match
        var match = new RouteMatch
        {
            Path = dbRoute.MatchPath,
            Methods = ParseMethods(dbRoute.MatchMethods),
            Hosts = ParseHosts(dbRoute.MatchHosts),
            Headers = ParseHeaders(dbRoute.MatchHeaders),
            QueryParameters = ParseQueryParameters(dbRoute.MatchQueryParameters),
        };

        // 解析 Transforms
        var transforms = BuildTransforms(dbRoute);

        // 解析 Metadata
        var metadata = ParseMetadata(dbRoute.Metadata);

        return new RouteConfig
        {
            RouteId = dbRoute.RouteId,
            ClusterId = dbRoute.ClusterId,
            Match = match,
            Order = dbRoute.Order,
            AuthorizationPolicy = string.IsNullOrEmpty(dbRoute.AuthorizationPolicy)
                ? null
                : dbRoute.AuthorizationPolicy,
            RateLimiterPolicy = string.IsNullOrEmpty(dbRoute.RateLimiterPolicy)
                ? null
                : dbRoute.RateLimiterPolicy,
            CorsPolicy = string.IsNullOrEmpty(dbRoute.CorsPolicy) ? null : dbRoute.CorsPolicy,
            Timeout = dbRoute.TimeoutSeconds.HasValue
                ? TimeSpan.FromSeconds(dbRoute.TimeoutSeconds.Value)
                : null,
            Transforms = transforms,
            Metadata = metadata,
        };
    }

    private ClusterConfig ConvertToClusterConfig(GatewayClusterRecord dbCluster)
    {
        // 解析目标地址
        var destinations = ParseDestinations(dbCluster.Destinations);

        // 健康检查配置
        HealthCheckConfig? healthCheck = null;
        if (dbCluster.HealthCheckEnabled)
        {
            healthCheck = new HealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = dbCluster.HealthCheckInterval.HasValue
                        ? TimeSpan.FromSeconds(dbCluster.HealthCheckInterval.Value)
                        : TimeSpan.FromSeconds(30),
                    Timeout = dbCluster.HealthCheckTimeout.HasValue
                        ? TimeSpan.FromSeconds(dbCluster.HealthCheckTimeout.Value)
                        : TimeSpan.FromSeconds(10),
                    Path = dbCluster.HealthCheckPath ?? "/health",
                },
            };
        }

        // Session 亲和性配置
        SessionAffinityConfig? sessionAffinity = null;
        if (dbCluster.SessionAffinityEnabled)
        {
            sessionAffinity = new SessionAffinityConfig
            {
                Enabled = true,
                Policy = dbCluster.SessionAffinityPolicy ?? "Cookie",
                AffinityKeyName = dbCluster.SessionAffinityKeyName ?? ".Yarp.Affinity",
            };
        }

        // HTTP 客户端配置
        HttpClientConfig? httpClient = null;
        if (dbCluster.MaxConnectionsPerServer.HasValue)
        {
            httpClient = new HttpClientConfig
            {
                MaxConnectionsPerServer = dbCluster.MaxConnectionsPerServer,
            };
        }

        // HTTP 请求配置
        ForwarderRequestConfig? httpRequest = null;
        if (dbCluster.RequestTimeoutSeconds.HasValue)
        {
            httpRequest = new ForwarderRequestConfig
            {
                ActivityTimeout = TimeSpan.FromSeconds(dbCluster.RequestTimeoutSeconds.Value),
            };
        }

        // 解析 Metadata
        var metadata = ParseMetadata(dbCluster.Metadata);

        return new ClusterConfig
        {
            ClusterId = dbCluster.ClusterId,
            LoadBalancingPolicy = dbCluster.LoadBalancingPolicy ?? LoadBalancingPolicies.RoundRobin,
            Destinations = destinations,
            HealthCheck = healthCheck,
            SessionAffinity = sessionAffinity,
            HttpClient = httpClient,
            HttpRequest = httpRequest,
            Metadata = metadata,
        };
    }

    private IReadOnlyList<string>? ParseMethods(string? methods)
    {
        if (string.IsNullOrWhiteSpace(methods))
            return null;

        return methods.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
    }

    private IReadOnlyList<string>? ParseHosts(string? hosts)
    {
        if (string.IsNullOrWhiteSpace(hosts))
            return null;

        return hosts.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
    }

    private IReadOnlyList<RouteHeader>? ParseHeaders(string? headersJson)
    {
        if (string.IsNullOrWhiteSpace(headersJson))
            return null;

        try
        {
            var headers = JsonSerializer.Deserialize<List<HeaderMatchConfig>>(headersJson);
            return headers
                ?.Select(h => new RouteHeader
                {
                    Name = h.Name,
                    Values = h.Values,
                    Mode = Enum.TryParse<HeaderMatchMode>(h.Mode, true, out var mode)
                        ? mode
                        : HeaderMatchMode.ExactHeader,
                    IsCaseSensitive = h.IsCaseSensitive,
                })
                .ToList();
        }
        catch
        {
            return null;
        }
    }

    private IReadOnlyList<RouteQueryParameter>? ParseQueryParameters(string? queryParamsJson)
    {
        if (string.IsNullOrWhiteSpace(queryParamsJson))
            return null;

        try
        {
            var queryParams = JsonSerializer.Deserialize<List<QueryParameterMatchConfig>>(
                queryParamsJson
            );
            return queryParams
                ?.Select(q => new RouteQueryParameter
                {
                    Name = q.Name,
                    Values = q.Values,
                    Mode = Enum.TryParse<QueryParameterMatchMode>(q.Mode, true, out var mode)
                        ? mode
                        : QueryParameterMatchMode.Exact,
                    IsCaseSensitive = q.IsCaseSensitive,
                })
                .ToList();
        }
        catch
        {
            return null;
        }
    }

    private IReadOnlyList<IReadOnlyDictionary<string, string>>? BuildTransforms(
        GatewayRouteRecord dbRoute
    )
    {
        var transforms = new List<Dictionary<string, string>>();

        // 移除路径前缀
        if (!string.IsNullOrEmpty(dbRoute.TransformPathRemovePrefix))
        {
            transforms.Add(
                new Dictionary<string, string>
                {
                    { "PathRemovePrefix", dbRoute.TransformPathRemovePrefix },
                }
            );
        }

        // 添加路径前缀
        if (!string.IsNullOrEmpty(dbRoute.TransformPathPrefix))
        {
            transforms.Add(
                new Dictionary<string, string> { { "PathPrefix", dbRoute.TransformPathPrefix } }
            );
        }

        // 请求头转换
        if (!string.IsNullOrEmpty(dbRoute.TransformRequestHeaders))
        {
            try
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    dbRoute.TransformRequestHeaders
                );
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        transforms.Add(
                            new Dictionary<string, string>
                            {
                                { "RequestHeader", header.Key },
                                { "Set", header.Value },
                            }
                        );
                    }
                }
            }
            catch { }
        }

        // 响应头转换
        if (!string.IsNullOrEmpty(dbRoute.TransformResponseHeaders))
        {
            try
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    dbRoute.TransformResponseHeaders
                );
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        transforms.Add(
                            new Dictionary<string, string>
                            {
                                { "ResponseHeader", header.Key },
                                { "Set", header.Value },
                            }
                        );
                    }
                }
            }
            catch { }
        }

        return transforms.Count > 0
            ? transforms.Select(t => (IReadOnlyDictionary<string, string>)t).ToList()
            : null;
    }

    private IReadOnlyDictionary<string, DestinationConfig>? ParseDestinations(
        string? destinationsJson
    )
    {
        if (string.IsNullOrWhiteSpace(destinationsJson))
            return null;

        try
        {
            // 支持两种格式：
            // 1. 简单数组格式: [{"destinationId": "dest1", "address": "http://..."}]
            // 2. 对象格式: {"dest1": {"Address": "http://..."}}

            var destinations = new Dictionary<string, DestinationConfig>();

            using var doc = JsonDocument.Parse(destinationsJson);

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    var destId =
                        item.GetProperty("destinationId").GetString() ?? item.GetProperty("id")
                            .GetString()
                        ?? Guid.NewGuid().ToString();
                    var address =
                        item.GetProperty("address").GetString() ?? item.GetProperty("Address")
                            .GetString();

                    if (!string.IsNullOrEmpty(address))
                    {
                        destinations[destId] = new DestinationConfig { Address = address };
                    }
                }
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var address = prop.Value.GetProperty("Address").GetString();
                    if (!string.IsNullOrEmpty(address))
                    {
                        destinations[prop.Name] = new DestinationConfig { Address = address };
                    }
                }
            }

            return destinations.Count > 0 ? destinations : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse destinations JSON: {Json}", destinationsJson);
            return null;
        }
    }

    private IReadOnlyDictionary<string, string>? ParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
        }
        catch
        {
            return null;
        }
    }

    #region Helper Classes

    private class HeaderMatchConfig
    {
        public string Name { get; set; } = "";
        public IReadOnlyList<string>? Values { get; set; }
        public string Mode { get; set; } = "ExactHeader";
        public bool IsCaseSensitive { get; set; }
    }

    private class QueryParameterMatchConfig
    {
        public string Name { get; set; } = "";
        public IReadOnlyList<string>? Values { get; set; }
        public string Mode { get; set; } = "Exact";
        public bool IsCaseSensitive { get; set; }
    }

    #endregion
}

#region Database Records

internal class GatewayRouteRecord
{
    public string RouteId { get; set; } = "";
    public string Name { get; set; } = "";
    public string ClusterId { get; set; } = "";
    public string MatchPath { get; set; } = "";
    public string? MatchMethods { get; set; }
    public string? MatchHosts { get; set; }
    public string? MatchHeaders { get; set; }
    public string? MatchQueryParameters { get; set; }
    public string? AuthorizationPolicy { get; set; }
    public string? RateLimiterPolicy { get; set; }
    public string? CorsPolicy { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int Order { get; set; }
    public string? TransformPathPrefix { get; set; }
    public string? TransformPathRemovePrefix { get; set; }
    public string? TransformRequestHeaders { get; set; }
    public string? TransformResponseHeaders { get; set; }
    public string? Metadata { get; set; }
}

internal class GatewayClusterRecord
{
    public string ClusterId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? LoadBalancingPolicy { get; set; }
    public string? Destinations { get; set; }
    public bool HealthCheckEnabled { get; set; }
    public int? HealthCheckInterval { get; set; }
    public int? HealthCheckTimeout { get; set; }
    public string? HealthCheckPath { get; set; }
    public bool SessionAffinityEnabled { get; set; }
    public string? SessionAffinityPolicy { get; set; }
    public string? SessionAffinityKeyName { get; set; }
    public int? MaxConnectionsPerServer { get; set; }
    public int? RequestTimeoutSeconds { get; set; }
    public string? Metadata { get; set; }
}

#endregion
