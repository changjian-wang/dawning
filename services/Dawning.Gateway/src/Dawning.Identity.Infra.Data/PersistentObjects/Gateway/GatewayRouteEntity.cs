using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Gateway
{
    /// <summary>
    /// 网关路由数据库实体
    /// </summary>
    [Table("gateway_routes")]
    public class GatewayRouteEntity
    {
        /// <summary>
        /// 路由唯一标识
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 路由ID（YARP配置中使用的标识符）
        /// </summary>
        [Column("route_id")]
        public string RouteId { get; set; } = string.Empty;

        /// <summary>
        /// 路由名称
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 路由描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 关联的集群ID
        /// </summary>
        [Column("cluster_id")]
        public string ClusterId { get; set; } = string.Empty;

        /// <summary>
        /// 匹配路径模式
        /// </summary>
        [Column("match_path")]
        public string MatchPath { get; set; } = string.Empty;

        /// <summary>
        /// 匹配的HTTP方法
        /// </summary>
        [Column("match_methods")]
        public string? MatchMethods { get; set; }

        /// <summary>
        /// 匹配的主机头
        /// </summary>
        [Column("match_hosts")]
        public string? MatchHosts { get; set; }

        /// <summary>
        /// 匹配的请求头（JSON格式）
        /// </summary>
        [Column("match_headers")]
        public string? MatchHeaders { get; set; }

        /// <summary>
        /// 匹配的查询参数（JSON格式）
        /// </summary>
        [Column("match_query_parameters")]
        public string? MatchQueryParameters { get; set; }

        /// <summary>
        /// 转换配置 - 路径前缀
        /// </summary>
        [Column("transform_path_prefix")]
        public string? TransformPathPrefix { get; set; }

        /// <summary>
        /// 转换配置 - 路径移除前缀
        /// </summary>
        [Column("transform_path_remove_prefix")]
        public string? TransformPathRemovePrefix { get; set; }

        /// <summary>
        /// 转换配置 - 请求头（JSON格式）
        /// </summary>
        [Column("transform_request_headers")]
        public string? TransformRequestHeaders { get; set; }

        /// <summary>
        /// 转换配置 - 响应头（JSON格式）
        /// </summary>
        [Column("transform_response_headers")]
        public string? TransformResponseHeaders { get; set; }

        /// <summary>
        /// 授权策略
        /// </summary>
        [Column("authorization_policy")]
        public string? AuthorizationPolicy { get; set; }

        /// <summary>
        /// 限流策略
        /// </summary>
        [Column("rate_limiter_policy")]
        public string? RateLimiterPolicy { get; set; }

        /// <summary>
        /// CORS策略
        /// </summary>
        [Column("cors_policy")]
        public string? CorsPolicy { get; set; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        [Column("timeout_seconds")]
        public int? TimeoutSeconds { get; set; }

        /// <summary>
        /// 路由顺序
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 元数据（JSON格式）
        /// </summary>
        [Column("metadata")]
        public string? Metadata { get; set; }

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
