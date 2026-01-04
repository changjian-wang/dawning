using System;
using System.ComponentModel.DataAnnotations;

namespace Dawning.Identity.Application.Dtos.Gateway
{
    /// <summary>
    /// Gateway Route DTO
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
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Create Gateway Route DTO
    /// </summary>
    public class CreateGatewayRouteDto
    {
        [Required(ErrorMessage = "Route ID is required")]
        [StringLength(100, ErrorMessage = "Route ID cannot exceed 100 characters")]
        public string RouteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route name is required")]
        [StringLength(200, ErrorMessage = "Route name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Cluster ID is required")]
        [StringLength(100, ErrorMessage = "Cluster ID cannot exceed 100 characters")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Match path is required")]
        [StringLength(500, ErrorMessage = "Match path cannot exceed 500 characters")]
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
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Update Gateway Route DTO
    /// </summary>
    public class UpdateGatewayRouteDto
    {
        [Required(ErrorMessage = "ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Route ID is required")]
        [StringLength(100, ErrorMessage = "Route ID cannot exceed 100 characters")]
        public string RouteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route name is required")]
        [StringLength(200, ErrorMessage = "Route name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Cluster ID is required")]
        [StringLength(100, ErrorMessage = "Cluster ID cannot exceed 100 characters")]
        public string ClusterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Match path is required")]
        [StringLength(500, ErrorMessage = "Match path cannot exceed 500 characters")]
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
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Metadata { get; set; }
    }
}
