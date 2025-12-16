using System;
using System.ComponentModel.DataAnnotations;

namespace Dawning.Identity.Application.Dtos.Gateway
{
    /// <summary>
    /// 网关路由 DTO
    /// </summary>
    public class GatewayRouteDto
    {
        public Guid Id { get; set; }
        public string RouteId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ClusterId { get; set; } = string.Empty;
        public string MatchPath { get; set; } = string.Empty;
        public string? MatchMethods { get; set; }
        public string? MatchHosts { get; set; }
        public string? MatchHeaders { get; set; }
        public string? MatchQueryParameters { get; set; }
        public string? TransformPathPrefix { get; set; }
        public string? TransformPathRemovePrefix { get; set; }
        public string? TransformRequestHeaders { get; set; }
        public string? TransformResponseHeaders { get; set; }
        public string? AuthorizationPolicy { get; set; }
        public string? RateLimiterPolicy { get; set; }
        public string? CorsPolicy { get; set; }
        public int? TimeoutSeconds { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// 创建网关路由 DTO
    /// </summary>
    public class CreateGatewayRouteDto
    {
        [Required(ErrorMessage = "路由ID不能为空")]
        [StringLength(100, ErrorMessage = "路由ID长度不能超过100")]
        public string RouteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "路由名称不能为空")]
        [StringLength(200, ErrorMessage = "路由名称长度不能超过200")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "描述长度不能超过1000")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "集群ID不能为空")]
        [StringLength(100, ErrorMessage = "集群ID长度不能超过100")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "匹配路径不能为空")]
        [StringLength(500, ErrorMessage = "匹配路径长度不能超过500")]
        public string MatchPath { get; set; } = string.Empty;

        public string? MatchMethods { get; set; }
        public string? MatchHosts { get; set; }
        public string? MatchHeaders { get; set; }
        public string? MatchQueryParameters { get; set; }
        public string? TransformPathPrefix { get; set; }
        public string? TransformPathRemovePrefix { get; set; }
        public string? TransformRequestHeaders { get; set; }
        public string? TransformResponseHeaders { get; set; }
        public string? AuthorizationPolicy { get; set; }
        public string? RateLimiterPolicy { get; set; }
        public string? CorsPolicy { get; set; }
        public int? TimeoutSeconds { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// 更新网关路由 DTO
    /// </summary>
    public class UpdateGatewayRouteDto
    {
        [Required(ErrorMessage = "ID不能为空")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "路由ID不能为空")]
        [StringLength(100, ErrorMessage = "路由ID长度不能超过100")]
        public string RouteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "路由名称不能为空")]
        [StringLength(200, ErrorMessage = "路由名称长度不能超过200")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "描述长度不能超过1000")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "集群ID不能为空")]
        [StringLength(100, ErrorMessage = "集群ID长度不能超过100")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "匹配路径不能为空")]
        [StringLength(500, ErrorMessage = "匹配路径长度不能超过500")]
        public string MatchPath { get; set; } = string.Empty;

        public string? MatchMethods { get; set; }
        public string? MatchHosts { get; set; }
        public string? MatchHeaders { get; set; }
        public string? MatchQueryParameters { get; set; }
        public string? TransformPathPrefix { get; set; }
        public string? TransformPathRemovePrefix { get; set; }
        public string? TransformRequestHeaders { get; set; }
        public string? TransformResponseHeaders { get; set; }
        public string? AuthorizationPolicy { get; set; }
        public string? RateLimiterPolicy { get; set; }
        public string? CorsPolicy { get; set; }
        public int? TimeoutSeconds { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Metadata { get; set; }
    }
}
