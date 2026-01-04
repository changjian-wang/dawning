using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// Gateway Cluster Aggregate Root
    /// Used for managing YARP reverse proxy backend cluster configuration
    /// </summary>
    public class GatewayCluster : IAggregateRoot
    {
        /// <summary>
        /// Cluster unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Cluster ID (identifier used in YARP configuration)
        /// </summary>
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// Cluster name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cluster description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Load balancing policy (RoundRobin, Random, LeastRequests, PowerOfTwoChoices, First)
        /// </summary>
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";

        /// <summary>
        /// Destination address list (JSON format)
        /// Format: [{"destinationId": "dest1", "address": "http://localhost:5001", "health": "http://localhost:5001/health"}]
        /// </summary>
        public string Destinations { get; set; } = "[]";

        /// <summary>
        /// Health check configuration - Enable active health check
        /// </summary>
        public bool HealthCheckEnabled { get; set; }

        /// <summary>
        /// Health check configuration - Check interval (seconds)
        /// </summary>
        public int? HealthCheckInterval { get; set; }

        /// <summary>
        /// Health check configuration - Timeout (seconds)
        /// </summary>
        public int? HealthCheckTimeout { get; set; }

        /// <summary>
        /// Health check configuration - Health check path
        /// </summary>
        public string? HealthCheckPath { get; set; }

        /// <summary>
        /// Health check configuration - Passive health check policy
        /// </summary>
        public string? PassiveHealthPolicy { get; set; }

        /// <summary>
        /// Session affinity configuration - Enable
        /// </summary>
        public bool SessionAffinityEnabled { get; set; }

        /// <summary>
        /// Session affinity configuration - Policy (Cookie, CustomHeader)
        /// </summary>
        public string? SessionAffinityPolicy { get; set; }

        /// <summary>
        /// Session affinity configuration - Affinity key name
        /// </summary>
        public string? SessionAffinityKeyName { get; set; }

        /// <summary>
        /// HTTP client configuration - Maximum connections per server
        /// </summary>
        public int? MaxConnectionsPerServer { get; set; }

        /// <summary>
        /// HTTP client configuration - Request timeout (seconds)
        /// </summary>
        public int? RequestTimeoutSeconds { get; set; }

        /// <summary>
        /// HTTP client configuration - Allowed HTTP versions (comma-separated, e.g., "1.1,2")
        /// </summary>
        public string? AllowedHttpVersions { get; set; }

        /// <summary>
        /// HTTP client configuration - Whether to validate SSL certificate
        /// </summary>
        public bool DangerousAcceptAnyServerCertificate { get; set; }

        /// <summary>
        /// Metadata (JSON format)
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

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
