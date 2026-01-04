using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Gateway
{
    /// <summary>
    /// Gateway cluster database entity
    /// </summary>
    [Table("gateway_clusters")]
    public class GatewayClusterEntity
    {
        /// <summary>
        /// Cluster unique identifier
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Cluster ID (identifier used in YARP configuration)
        /// </summary>
        [Column("cluster_id")]
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// Cluster name
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cluster description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Load balancing policy
        /// </summary>
        [Column("load_balancing_policy")]
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";

        /// <summary>
        /// Destination addresses list (JSON format)
        /// </summary>
        [Column("destinations")]
        public string Destinations { get; set; } = "[]";

        /// <summary>
        /// Health check configuration - whether enabled
        /// </summary>
        [Column("health_check_enabled")]
        public bool HealthCheckEnabled { get; set; }

        /// <summary>
        /// Health check configuration - check interval (seconds)
        /// </summary>
        [Column("health_check_interval")]
        public int? HealthCheckInterval { get; set; }

        /// <summary>
        /// Health check configuration - timeout (seconds)
        /// </summary>
        [Column("health_check_timeout")]
        public int? HealthCheckTimeout { get; set; }

        /// <summary>
        /// Health check configuration - health check path
        /// </summary>
        [Column("health_check_path")]
        public string? HealthCheckPath { get; set; }

        /// <summary>
        /// Passive health check policy
        /// </summary>
        [Column("passive_health_policy")]
        public string? PassiveHealthPolicy { get; set; }

        /// <summary>
        /// Session affinity configuration - whether enabled
        /// </summary>
        [Column("session_affinity_enabled")]
        public bool SessionAffinityEnabled { get; set; }

        /// <summary>
        /// Session affinity configuration - policy
        /// </summary>
        [Column("session_affinity_policy")]
        public string? SessionAffinityPolicy { get; set; }

        /// <summary>
        /// Session affinity configuration - affinity key name
        /// </summary>
        [Column("session_affinity_key_name")]
        public string? SessionAffinityKeyName { get; set; }

        /// <summary>
        /// HTTP client configuration - maximum connections
        /// </summary>
        [Column("max_connections_per_server")]
        public int? MaxConnectionsPerServer { get; set; }

        /// <summary>
        /// HTTP client configuration - request timeout (seconds)
        /// </summary>
        [Column("request_timeout_seconds")]
        public int? RequestTimeoutSeconds { get; set; }

        /// <summary>
        /// HTTP client configuration - allowed HTTP versions
        /// </summary>
        [Column("allowed_http_versions")]
        public string? AllowedHttpVersions { get; set; }

        /// <summary>
        /// HTTP client configuration - whether to validate SSL certificate
        /// </summary>
        [Column("dangerous_accept_any_server_certificate")]
        public bool DangerousAcceptAnyServerCertificate { get; set; }

        /// <summary>
        /// Metadata (JSON format)
        /// </summary>
        [Column("metadata")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Creator
        /// </summary>
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Updater
        /// </summary>
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
