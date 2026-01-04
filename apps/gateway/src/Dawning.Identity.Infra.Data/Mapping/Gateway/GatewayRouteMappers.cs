using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// Gateway route entity and domain model mapper
    /// </summary>
    public static class GatewayRouteMappers
    {
        private static IMapper Mapper { get; }

        static GatewayRouteMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<GatewayRouteProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static GatewayRoute ToModel(this GatewayRouteEntity entity)
        {
            return Mapper.Map<GatewayRoute>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static GatewayRouteEntity ToEntity(this GatewayRoute model)
        {
            return Mapper.Map<GatewayRouteEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<GatewayRoute> ToModels(this IEnumerable<GatewayRouteEntity> entities)
        {
            return Mapper.Map<IEnumerable<GatewayRoute>>(entities);
        }

        /// <summary>
        /// Convert domain model collection to entity collection
        /// </summary>
        public static IEnumerable<GatewayRouteEntity> ToEntities(this IEnumerable<GatewayRoute> models)
        {
            return Mapper.Map<IEnumerable<GatewayRouteEntity>>(models);
        }
    }
}
