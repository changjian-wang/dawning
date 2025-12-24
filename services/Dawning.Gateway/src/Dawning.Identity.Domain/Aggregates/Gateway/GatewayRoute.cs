using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// 网关路由聚合根
    /// 用于管理 YARP 反向代理的路由配置
    /// </summary>
    public class GatewayRoute : IAggregateRoot
    {
        /// <summary>
        /// 路由唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 路由ID（YARP配置中使用的标识符）
        /// </summary>
        public string RouteId { get; set; } = string.Empty;

        /// <summary>
        /// 路由名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 路由描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 关联的集群ID
        /// </summary>
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// 匹配路径模式 (例如: "/api/{**catch-all}")
        /// </summary>
        public string MatchPath { get; set; } = string.Empty;

        /// <summary>
        /// 匹配的HTTP方法（逗号分隔，例如: "GET,POST,PUT,DELETE"）
        /// 为空表示匹配所有方法
        /// </summary>
        public string? MatchMethods { get; set; }

        /// <summary>
        /// 匹配的主机头（逗号分隔）
        /// 为空表示匹配所有主机
        /// </summary>
        public string? MatchHosts { get; set; }

        /// <summary>
        /// 匹配的请求头（JSON格式）
        /// </summary>
        public string? MatchHeaders { get; set; }

        /// <summary>
        /// 匹配的查询参数（JSON格式）
        /// </summary>
        public string? MatchQueryParameters { get; set; }

        /// <summary>
        /// 转换配置 - 路径前缀（例如: "/api"）
        /// </summary>
        public string? TransformPathPrefix { get; set; }

        /// <summary>
        /// 转换配置 - 路径移除前缀
        /// </summary>
        public string? TransformPathRemovePrefix { get; set; }

        /// <summary>
        /// 转换配置 - 请求头（JSON格式）
        /// </summary>
        public string? TransformRequestHeaders { get; set; }

        /// <summary>
        /// 转换配置 - 响应头（JSON格式）
        /// </summary>
        public string? TransformResponseHeaders { get; set; }

        /// <summary>
        /// 授权策略
        /// </summary>
        public string? AuthorizationPolicy { get; set; }

        /// <summary>
        /// 限流策略
        /// </summary>
        public string? RateLimiterPolicy { get; set; }

        /// <summary>
        /// CORS策略
        /// </summary>
        public string? CorsPolicy { get; set; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int? TimeoutSeconds { get; set; }

        /// <summary>
        /// 路由顺序（越小越优先）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 元数据（JSON格式）
        /// </summary>
        public string? Metadata { get; set; }

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
