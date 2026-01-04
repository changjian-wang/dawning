using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// Gateway Route Aggregate Root
    /// Used for managing YARP reverse proxy route configuration
    /// </summary>
    public class GatewayRoute : IAggregateRoot
    {
        /// <summary>
        /// Route unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Route ID (identifier used in YARP configuration)
        /// </summary>
        public string RouteId { get; set; } = string.Empty;

        /// <summary>
        /// Route name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Route description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Associated cluster ID
        /// </summary>
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// Match path pattern (e.g., "/api/{**catch-all}")
        /// </summary>
        public string MatchPath { get; set; } = string.Empty;

        /// <summary>
        /// Matched HTTP methods (comma-separated, e.g., "GET,POST,PUT,DELETE")
        /// Empty means match all methods
        /// </summary>
        public string? MatchMethods { get; set; }

        /// <summary>
        /// Matched host headers (comma-separated)
        /// Empty means match all hosts
        /// </summary>
        public string? MatchHosts { get; set; }

        /// <summary>
        /// Matched request headers (JSON format)
        /// </summary>
        public string? MatchHeaders { get; set; }

        /// <summary>
        /// Matched query parameters (JSON format)
        /// </summary>
        public string? MatchQueryParameters { get; set; }

        /// <summary>
        /// Transform configuration - Path prefix (e.g., "/api")
        /// </summary>
        public string? TransformPathPrefix { get; set; }

        /// <summary>
        /// Transform configuration - Path remove prefix
        /// </summary>
        public string? TransformPathRemovePrefix { get; set; }

        /// <summary>
        /// Transform configuration - Request headers (JSON format)
        /// </summary>
        public string? TransformRequestHeaders { get; set; }

        /// <summary>
        /// Transform configuration - Response headers (JSON format)
        /// </summary>
        public string? TransformResponseHeaders { get; set; }

        /// <summary>
        /// Authorization policy
        /// </summary>
        public string? AuthorizationPolicy { get; set; }

        /// <summary>
        /// Rate limiter policy
        /// </summary>
        public string? RateLimiterPolicy { get; set; }

        /// <summary>
        /// CORS policy
        /// </summary>
        public string? CorsPolicy { get; set; }

        /// <summary>
        /// Timeout (seconds)
        /// </summary>
        public int? TimeoutSeconds { get; set; }

        /// <summary>
        /// Route order (smaller value has higher priority)
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Metadata (JSON format)
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Created by
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Updated by
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
