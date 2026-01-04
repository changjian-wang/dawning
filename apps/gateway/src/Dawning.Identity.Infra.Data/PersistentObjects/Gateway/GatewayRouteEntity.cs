using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Gateway
{
    /// <summary>
    /// Gateway route database entity
    /// </summary>
    [Table("gateway_routes")]
    public class GatewayRouteEntity
    {
        /// <summary>
        /// Route unique identifier
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Route ID (identifier used in YARP configuration)
        /// </summary>
        [Column("route_id")]
        public string RouteId { get; set; } = string.Empty;

        /// <summary>
        /// Route name
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Route description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Associated cluster ID
        /// </summary>
        [Column("cluster_id")]
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// Match path pattern
        /// </summary>
        [Column("match_path")]
        public string MatchPath { get; set; } = string.Empty;

        /// <summary>
        /// Match HTTP methods
        /// </summary>
        [Column("match_methods")]
        public string? MatchMethods { get; set; }

        /// <summary>
        /// Match host headers
        /// </summary>
        [Column("match_hosts")]
        public string? MatchHosts { get; set; }

        /// <summary>
        /// Match request headers (JSON format)
        /// </summary>
        [Column("match_headers")]
        public string? MatchHeaders { get; set; }

        /// <summary>
        /// Match query parameters (JSON format)
        /// </summary>
        [Column("match_query_parameters")]
        public string? MatchQueryParameters { get; set; }

        /// <summary>
        /// Transform configuration - path prefix
        /// </summary>
        [Column("transform_path_prefix")]
        public string? TransformPathPrefix { get; set; }

        /// <summary>
        /// Transform configuration - path remove prefix
        /// </summary>
        [Column("transform_path_remove_prefix")]
        public string? TransformPathRemovePrefix { get; set; }

        /// <summary>
        /// Transform configuration - request headers (JSON format)
        /// </summary>
        [Column("transform_request_headers")]
        public string? TransformRequestHeaders { get; set; }

        /// <summary>
        /// Transform configuration - response headers (JSON format)
        /// </summary>
        [Column("transform_response_headers")]
        public string? TransformResponseHeaders { get; set; }

        /// <summary>
        /// Authorization policy
        /// </summary>
        [Column("authorization_policy")]
        public string? AuthorizationPolicy { get; set; }

        /// <summary>
        /// Rate limiter policy
        /// </summary>
        [Column("rate_limiter_policy")]
        public string? RateLimiterPolicy { get; set; }

        /// <summary>
        /// CORS policy
        /// </summary>
        [Column("cors_policy")]
        public string? CorsPolicy { get; set; }

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        [Column("timeout_seconds")]
        public int? TimeoutSeconds { get; set; }

        /// <summary>
        /// Route order
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Metadata (JSON format)
        /// </summary>
        [Column("metadata")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Created at
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated at
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Created by
        /// </summary>
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Updated by
        /// </summary>
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
