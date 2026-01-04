using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// Role entity and domain model mapping configuration
    /// </summary>
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            // Entity -> Domain Model
            CreateMap<RoleEntity, Role>();

            // Domain Model -> Entity
            CreateMap<Role, RoleEntity>();
        }
    }
}
