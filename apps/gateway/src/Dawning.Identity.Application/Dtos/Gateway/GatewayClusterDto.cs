using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dawning.Identity.Application.Dtos.Gateway
{
    /// <summary>
    /// 网关集群 DTO
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
    /// 集群目标服务器配置
    /// </summary>
    public class ClusterDestinationDto
    {
        public string DestinationId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Health { get; set; }
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// 创建网关集群 DTO
    /// </summary>
    public class CreateGatewayClusterDto
    {
        [Required(ErrorMessage = "集群ID不能为空")]
        [StringLength(100, ErrorMessage = "集群ID长度不能超过100")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "集群名称不能为空")]
        [StringLength(200, ErrorMessage = "集群名称长度不能超过200")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "描述长度不能超过1000")]
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
    /// 更新网关集群 DTO
    /// </summary>
    public class UpdateGatewayClusterDto
    {
        [Required(ErrorMessage = "ID不能为空")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "集群ID不能为空")]
        [StringLength(100, ErrorMessage = "集群ID长度不能超过100")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "集群名称不能为空")]
        [StringLength(200, ErrorMessage = "集群名称长度不能超过200")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "描述长度不能超过1000")]
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
    /// 集群选项 DTO（用于下拉选择）
    /// </summary>
    public class ClusterOptionDto
    {
        public string ClusterId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }
}
