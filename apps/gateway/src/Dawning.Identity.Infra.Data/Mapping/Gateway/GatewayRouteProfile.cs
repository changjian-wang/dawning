using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// GatewayRoute entity and domain model mapping configuration
    /// </summary>
    public class GatewayRouteProfile : Profile
    {
        public GatewayRouteProfile()
        {
            // Entity -> Domain Model
            CreateMap<GatewayRouteEntity, GatewayRoute>();

            // Domain Model -> Entity
            CreateMap<GatewayRoute, GatewayRouteEntity>();
        }
    }
}
