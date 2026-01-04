using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dawning.Identity.Application.Dtos.Gateway
{
    /// <summary>
    /// Gateway Cluster DTO
    /// </summary>
    public class GatewayClusterDto
    {
        public Guid Id { get; set; }
        public string ClusterId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";
        public string Destinations { get; set; } = "[]";
        public bool HealthCheckEnabled { get; set; }
        public int? HealthCheckInterval { get; set; }
        public int? HealthCheckTimeout { get; set; }
        public string? HealthCheckPath { get; set; }
        public string? PassiveHealthPolicy { get; set; }
        public bool SessionAffinityEnabled { get; set; }
        public string? SessionAffinityPolicy { get; set; }
        public string? SessionAffinityKeyName { get; set; }
        public int? MaxConnectionsPerServer { get; set; }
        public int? RequestTimeoutSeconds { get; set; }
        public string? AllowedHttpVersions { get; set; }
        public bool DangerousAcceptAnyServerCertificate { get; set; }
        public string? Metadata { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Cluster Destination Server Configuration
    /// </summary>
    public class ClusterDestinationDto
    {
        public string DestinationId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Health { get; set; }
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Create Gateway Cluster DTO
    /// </summary>
    public class CreateGatewayClusterDto
    {
        [Required(ErrorMessage = "Cluster ID is required")]
        [StringLength(100, ErrorMessage = "Cluster ID cannot exceed 100 characters")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cluster name is required")]
        [StringLength(200, ErrorMessage = "Cluster name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public string LoadBalancingPolicy { get; set; } = "RoundRobin";
        public string Destinations { get; set; } = "[]";
        public bool HealthCheckEnabled { get; set; }
        public int? HealthCheckInterval { get; set; }
        public int? HealthCheckTimeout { get; set; }
        public string? HealthCheckPath { get; set; }
        public string? PassiveHealthPolicy { get; set; }
        public bool SessionAffinityEnabled { get; set; }
        public string? SessionAffinityPolicy { get; set; }
        public string? SessionAffinityKeyName { get; set; }
        public int? MaxConnectionsPerServer { get; set; }
        public int? RequestTimeoutSeconds { get; set; }
        public string? AllowedHttpVersions { get; set; }
        public bool DangerousAcceptAnyServerCertificate { get; set; }
        public string? Metadata { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Update Gateway Cluster DTO
    /// </summary>
    public class UpdateGatewayClusterDto
    {
        [Required(ErrorMessage = "ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Cluster ID is required")]
        [StringLength(100, ErrorMessage = "Cluster ID cannot exceed 100 characters")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cluster name is required")]
        [StringLength(200, ErrorMessage = "Cluster name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public string LoadBalancingPolicy { get; set; } = "RoundRobin";
        public string Destinations { get; set; } = "[]";
        public bool HealthCheckEnabled { get; set; }
        public int? HealthCheckInterval { get; set; }
        public int? HealthCheckTimeout { get; set; }
        public string? HealthCheckPath { get; set; }
        public string? PassiveHealthPolicy { get; set; }
        public bool SessionAffinityEnabled { get; set; }
        public string? SessionAffinityPolicy { get; set; }
        public string? SessionAffinityKeyName { get; set; }
        public int? MaxConnectionsPerServer { get; set; }
        public int? RequestTimeoutSeconds { get; set; }
        public string? AllowedHttpVersions { get; set; }
        public bool DangerousAcceptAnyServerCertificate { get; set; }
        public string? Metadata { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Cluster Option DTO (for dropdown selection)
    /// </summary>
    public class ClusterOptionDto
    {
        public string ClusterId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }
}
