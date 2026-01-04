using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.LoadBalancing;

namespace Dawning.Identity.Application.Services.Gateway
{
    /// <summary>
    /// Gateway configuration service - reads YARP configuration from database
    /// </summary>
    public class GatewayConfigService : IGatewayConfigService
    {
        private readonly IGatewayRouteRepository _routeRepository;
        private readonly IGatewayClusterRepository _clusterRepository;
        private readonly ILogger<GatewayConfigService> _logger;

        public GatewayConfigService(
            IGatewayRouteRepository routeRepository,
            IGatewayClusterRepository clusterRepository,
            ILogger<GatewayConfigService> logger
        )
        {
            _routeRepository = routeRepository;
            _clusterRepository = clusterRepository;
            _logger = logger;
        }

        public async Task<IReadOnlyList<RouteConfig>> GetRoutesAsync()
        {
            var dbRoutes = await _routeRepository.GetAllEnabledAsync();

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
            var dbClusters = await _clusterRepository.GetAllEnabledAsync();

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

        private RouteConfig ConvertToRouteConfig(
            Dawning.Identity.Domain.Aggregates.Gateway.GatewayRoute dbRoute
        )
        {
            // Parse Match
            var match = new RouteMatch
            {
                Path = dbRoute.MatchPath,
                Methods = ParseMethods(dbRoute.MatchMethods),
                Hosts = ParseHosts(dbRoute.MatchHosts),
                Headers = ParseHeaders(dbRoute.MatchHeaders),
                QueryParameters = ParseQueryParameters(dbRoute.MatchQueryParameters),
            };

            // Parse Transforms
            var transforms = BuildTransforms(dbRoute);

            // Parse Metadata
            var metadata = ParseMetadata(dbRoute.Metadata);

            return new RouteConfig
            {
                RouteId = dbRoute.RouteId,
                ClusterId = dbRoute.ClusterId,
                Match = match,
                Order = dbRoute.SortOrder,
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

        private ClusterConfig ConvertToClusterConfig(
            Dawning.Identity.Domain.Aggregates.Gateway.GatewayCluster dbCluster
        )
        {
            // Parse destination addresses
            var destinations = ParseDestinations(dbCluster.Destinations);

            // Health check configuration
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

            // Session affinity configuration
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

            // HTTP client configuration
            HttpClientConfig? httpClient = null;
            if (dbCluster.MaxConnectionsPerServer.HasValue)
            {
                httpClient = new HttpClientConfig
                {
                    MaxConnectionsPerServer = dbCluster.MaxConnectionsPerServer,
                };
            }

            // HTTP request configuration
            ForwarderRequestConfig? httpRequest = null;
            if (dbCluster.RequestTimeoutSeconds.HasValue)
            {
                httpRequest = new ForwarderRequestConfig
                {
                    ActivityTimeout = TimeSpan.FromSeconds(dbCluster.RequestTimeoutSeconds.Value),
                };
            }

            // Parse Metadata
            var metadata = ParseMetadata(dbCluster.Metadata);

            return new ClusterConfig
            {
                ClusterId = dbCluster.ClusterId,
                LoadBalancingPolicy =
                    dbCluster.LoadBalancingPolicy ?? LoadBalancingPolicies.RoundRobin,
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
            GatewayRoute dbRoute
        )
        {
            var transforms = new List<Dictionary<string, string>>();

            // Remove path prefix
            if (!string.IsNullOrEmpty(dbRoute.TransformPathRemovePrefix))
            {
                transforms.Add(
                    new Dictionary<string, string>
                    {
                        { "PathRemovePrefix", dbRoute.TransformPathRemovePrefix },
                    }
                );
            }

            // Add path prefix
            if (!string.IsNullOrEmpty(dbRoute.TransformPathPrefix))
            {
                transforms.Add(
                    new Dictionary<string, string> { { "PathPrefix", dbRoute.TransformPathPrefix } }
                );
            }

            // Request header transforms
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

            // Response header transforms
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
                // Support two formats:
                // 1. Simple array format: [{"destinationId": "dest1", "address": "http://..."}]
                // 2. Object format: {"dest1": {"Address": "http://..."}}

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
                _logger.LogWarning(
                    ex,
                    "Failed to parse destinations JSON: {Json}",
                    destinationsJson
                );
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

        // ===== Helper Classes =====

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
    }
}
