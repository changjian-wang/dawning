using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// 网关路由映射器
    /// </summary>
    public static class GatewayRouteMappers
    {
        /// <summary>
        /// 将数据库实体转换为领域模型
        /// </summary>
        public static GatewayRoute ToModel(this GatewayRouteEntity entity)
        {
            return new GatewayRoute
            {
                Id = entity.Id,
                RouteId = entity.RouteId,
                Name = entity.Name,
                Description = entity.Description,
                ClusterId = entity.ClusterId,
                MatchPath = entity.MatchPath,
                MatchMethods = entity.MatchMethods,
                MatchHosts = entity.MatchHosts,
                MatchHeaders = entity.MatchHeaders,
                MatchQueryParameters = entity.MatchQueryParameters,
                TransformPathPrefix = entity.TransformPathPrefix,
                TransformPathRemovePrefix = entity.TransformPathRemovePrefix,
                TransformRequestHeaders = entity.TransformRequestHeaders,
                TransformResponseHeaders = entity.TransformResponseHeaders,
                AuthorizationPolicy = entity.AuthorizationPolicy,
                RateLimiterPolicy = entity.RateLimiterPolicy,
                CorsPolicy = entity.CorsPolicy,
                TimeoutSeconds = entity.TimeoutSeconds,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                Metadata = entity.Metadata,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
            };
        }

        /// <summary>
        /// 将领域模型转换为数据库实体
        /// </summary>
        public static GatewayRouteEntity ToEntity(this GatewayRoute model)
        {
            return new GatewayRouteEntity
            {
                Id = model.Id,
                RouteId = model.RouteId,
                Name = model.Name,
                Description = model.Description,
                ClusterId = model.ClusterId,
                MatchPath = model.MatchPath,
                MatchMethods = model.MatchMethods,
                MatchHosts = model.MatchHosts,
                MatchHeaders = model.MatchHeaders,
                MatchQueryParameters = model.MatchQueryParameters,
                TransformPathPrefix = model.TransformPathPrefix,
                TransformPathRemovePrefix = model.TransformPathRemovePrefix,
                TransformRequestHeaders = model.TransformRequestHeaders,
                TransformResponseHeaders = model.TransformResponseHeaders,
                AuthorizationPolicy = model.AuthorizationPolicy,
                RateLimiterPolicy = model.RateLimiterPolicy,
                CorsPolicy = model.CorsPolicy,
                TimeoutSeconds = model.TimeoutSeconds,
                SortOrder = model.SortOrder,
                IsEnabled = model.IsEnabled,
                Metadata = model.Metadata,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedBy = model.UpdatedBy,
            };
        }

        /// <summary>
        /// 批量将数据库实体转换为领域模型
        /// </summary>
        public static IEnumerable<GatewayRoute> ToModels(
            this IEnumerable<GatewayRouteEntity> entities
        )
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// 批量将领域模型转换为数据库实体
        /// </summary>
        public static IEnumerable<GatewayRouteEntity> ToEntities(
            this IEnumerable<GatewayRoute> models
        )
        {
            return models.Select(m => m.ToEntity());
        }
    }
}
