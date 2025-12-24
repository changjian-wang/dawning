using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// 网关集群映射器
    /// </summary>
    public static class GatewayClusterMappers
    {
        /// <summary>
        /// 将数据库实体转换为领域模型
        /// </summary>
        public static GatewayCluster ToModel(this GatewayClusterEntity entity)
        {
            return new GatewayCluster
            {
                Id = entity.Id,
                ClusterId = entity.ClusterId,
                Name = entity.Name,
                Description = entity.Description,
                LoadBalancingPolicy = entity.LoadBalancingPolicy,
                Destinations = entity.Destinations,
                HealthCheckEnabled = entity.HealthCheckEnabled,
                HealthCheckInterval = entity.HealthCheckInterval,
                HealthCheckTimeout = entity.HealthCheckTimeout,
                HealthCheckPath = entity.HealthCheckPath,
                PassiveHealthPolicy = entity.PassiveHealthPolicy,
                SessionAffinityEnabled = entity.SessionAffinityEnabled,
                SessionAffinityPolicy = entity.SessionAffinityPolicy,
                SessionAffinityKeyName = entity.SessionAffinityKeyName,
                MaxConnectionsPerServer = entity.MaxConnectionsPerServer,
                RequestTimeoutSeconds = entity.RequestTimeoutSeconds,
                AllowedHttpVersions = entity.AllowedHttpVersions,
                DangerousAcceptAnyServerCertificate = entity.DangerousAcceptAnyServerCertificate,
                Metadata = entity.Metadata,
                IsEnabled = entity.IsEnabled,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
            };
        }

        /// <summary>
        /// 将领域模型转换为数据库实体
        /// </summary>
        public static GatewayClusterEntity ToEntity(this GatewayCluster model)
        {
            return new GatewayClusterEntity
            {
                Id = model.Id,
                ClusterId = model.ClusterId,
                Name = model.Name,
                Description = model.Description,
                LoadBalancingPolicy = model.LoadBalancingPolicy,
                Destinations = model.Destinations,
                HealthCheckEnabled = model.HealthCheckEnabled,
                HealthCheckInterval = model.HealthCheckInterval,
                HealthCheckTimeout = model.HealthCheckTimeout,
                HealthCheckPath = model.HealthCheckPath,
                PassiveHealthPolicy = model.PassiveHealthPolicy,
                SessionAffinityEnabled = model.SessionAffinityEnabled,
                SessionAffinityPolicy = model.SessionAffinityPolicy,
                SessionAffinityKeyName = model.SessionAffinityKeyName,
                MaxConnectionsPerServer = model.MaxConnectionsPerServer,
                RequestTimeoutSeconds = model.RequestTimeoutSeconds,
                AllowedHttpVersions = model.AllowedHttpVersions,
                DangerousAcceptAnyServerCertificate = model.DangerousAcceptAnyServerCertificate,
                Metadata = model.Metadata,
                IsEnabled = model.IsEnabled,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedBy = model.UpdatedBy,
            };
        }

        /// <summary>
        /// 批量将数据库实体转换为领域模型
        /// </summary>
        public static IEnumerable<GatewayCluster> ToModels(
            this IEnumerable<GatewayClusterEntity> entities
        )
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// 批量将领域模型转换为数据库实体
        /// </summary>
        public static IEnumerable<GatewayClusterEntity> ToEntities(
            this IEnumerable<GatewayCluster> models
        )
        {
            return models.Select(m => m.ToEntity());
        }
    }
}
