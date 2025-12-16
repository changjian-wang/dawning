using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// 网关集群聚合根
    /// 用于管理 YARP 反向代理的后端集群配置
    /// </summary>
    public class GatewayCluster : IAggregateRoot
    {
        /// <summary>
        /// 集群唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 集群ID（YARP配置中使用的标识符）
        /// </summary>
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// 集群名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 集群描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 负载均衡策略 (RoundRobin, Random, LeastRequests, PowerOfTwoChoices, First)
        /// </summary>
        public string LoadBalancingPolicy { get; set; } = "RoundRobin";

        /// <summary>
        /// 目标地址列表（JSON格式）
        /// 格式: [{"destinationId": "dest1", "address": "http://localhost:5001", "health": "http://localhost:5001/health"}]
        /// </summary>
        public string Destinations { get; set; } = "[]";

        /// <summary>
        /// 健康检查配置 - 是否启用主动健康检查
        /// </summary>
        public bool HealthCheckEnabled { get; set; }

        /// <summary>
        /// 健康检查配置 - 检查间隔（秒）
        /// </summary>
        public int? HealthCheckInterval { get; set; }

        /// <summary>
        /// 健康检查配置 - 超时时间（秒）
        /// </summary>
        public int? HealthCheckTimeout { get; set; }

        /// <summary>
        /// 健康检查配置 - 健康检查路径
        /// </summary>
        public string? HealthCheckPath { get; set; }

        /// <summary>
        /// 健康检查配置 - 被动健康检查策略
        /// </summary>
        public string? PassiveHealthPolicy { get; set; }

        /// <summary>
        /// 会话亲和性配置 - 是否启用
        /// </summary>
        public bool SessionAffinityEnabled { get; set; }

        /// <summary>
        /// 会话亲和性配置 - 策略 (Cookie, CustomHeader)
        /// </summary>
        public string? SessionAffinityPolicy { get; set; }

        /// <summary>
        /// 会话亲和性配置 - 亲和键名
        /// </summary>
        public string? SessionAffinityKeyName { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 最大连接数
        /// </summary>
        public int? MaxConnectionsPerServer { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 请求超时（秒）
        /// </summary>
        public int? RequestTimeoutSeconds { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 允许的HTTP版本（逗号分隔，如 "1.1,2"）
        /// </summary>
        public string? AllowedHttpVersions { get; set; }

        /// <summary>
        /// HTTP客户端配置 - 是否验证SSL证书
        /// </summary>
        public bool DangerousAcceptAnyServerCertificate { get; set; }

        /// <summary>
        /// 元数据（JSON格式）
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
