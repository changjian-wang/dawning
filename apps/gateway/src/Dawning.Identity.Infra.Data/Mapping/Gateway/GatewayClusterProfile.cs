using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// GatewayCluster entity and domain model mapping configuration
    /// </summary>
    public class GatewayClusterProfile : Profile
    {
        public GatewayClusterProfile()
        {
            // Entity -> Domain Model
            CreateMap<GatewayClusterEntity, GatewayCluster>();

            // Domain Model -> Entity
            CreateMap<GatewayCluster, GatewayClusterEntity>();
        }
    }
}
