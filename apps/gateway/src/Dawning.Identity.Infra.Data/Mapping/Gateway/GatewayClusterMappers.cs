using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// Gateway cluster entity and domain model mapper
    /// </summary>
    public static class GatewayClusterMappers
    {
        private static IMapper Mapper { get; }

        static GatewayClusterMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<GatewayClusterProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static GatewayCluster ToModel(this GatewayClusterEntity entity)
        {
            return Mapper.Map<GatewayCluster>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static GatewayClusterEntity ToEntity(this GatewayCluster model)
        {
            return Mapper.Map<GatewayClusterEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<GatewayCluster> ToModels(this IEnumerable<GatewayClusterEntity> entities)
        {
            return Mapper.Map<IEnumerable<GatewayCluster>>(entities);
        }

        /// <summary>
        /// Convert domain model collection to entity collection
        /// </summary>
        public static IEnumerable<GatewayClusterEntity> ToEntities(this IEnumerable<GatewayCluster> models)
        {
            return Mapper.Map<IEnumerable<GatewayClusterEntity>>(models);
        }
    }
}
