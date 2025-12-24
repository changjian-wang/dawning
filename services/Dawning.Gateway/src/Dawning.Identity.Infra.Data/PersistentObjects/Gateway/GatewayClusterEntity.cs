using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Gateway
{
    /// <summary>
    /// 网关集群数据库实体
    /// </summary>
    [Table("gateway_clusters")]
    public class GatewayClusterEntity
    {
        /// <summary>
        /// 集群唯一标识
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 集群ID（YARP配置中使用的标识符）
        /// </summary>
        [Column("cluster_id")]
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// 集群名称
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 集群描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 负载均衡策略
        /// </summary>
        [Column("load_balancing_policy")]
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";

        /// <summary>
        /// 目标地址列表（JSON格式）
        /// </summary>
        [Column("destinations")]
        public string Destinations { get; set; } = "[]";

        /// <summary>
        /// 健康检查配置 - 是否启用
        /// </summary>
        [Column("health_check_enabled")]
        public bool HealthCheckEnabled { get; set; }

        /// <summary>
        /// 健康检查配置 - 检查间隔（秒）
        /// </summary>
        [Column("health_check_interval")]
        public int? HealthCheckInterval { get; set; }

        /// <summary>
        /// 健康检查配置 - 超时时间（秒）
        /// </summary>
        [Column("health_check_timeout")]
        public int? HealthCheckTimeout { get; set; }

        /// <summary>
        /// 健康检查配置 - 健康检查路径
        /// </summary>
        [Column("health_check_path")]
        public string? HealthCheckPath { get; set; }

        /// <summary>
        /// 被动健康检查策略
        /// </summary>
        [Column("passive_health_policy")]
        public string? PassiveHealthPolicy { get; set; }

        /// <summary>
        /// 会话亲和性配置 - 是否启用
        /// </summary>
        [Column("session_affinity_enabled")]
        public bool SessionAffinityEnabled { get; set; }

        /// <summary>
        /// 会话亲和性配置 - 策略
        /// </summary>
        [Column("session_affinity_policy")]
        public string? SessionAffinityPolicy { get; set; }

        /// <summary>
        /// 会话亲和性配置 - 亲和键名
        /// </summary>
        [Column("session_affinity_key_name")]
        public string? SessionAffinityKeyName { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 最大连接数
        /// </summary>
        [Column("max_connections_per_server")]
        public int? MaxConnectionsPerServer { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 请求超时（秒）
        /// </summary>
        [Column("request_timeout_seconds")]
        public int? RequestTimeoutSeconds { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 允许的HTTP版本
        /// </summary>
        [Column("allowed_http_versions")]
        public string? AllowedHttpVersions { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 是否验证SSL证书
        /// </summary>
        [Column("dangerous_accept_any_server_certificate")]
        public bool DangerousAcceptAnyServerCertificate { get; set; }

        /// <summary>
        /// 元数据（JSON格式）
        /// </summary>
        [Column("metadata")]
        public string? Metadata { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
