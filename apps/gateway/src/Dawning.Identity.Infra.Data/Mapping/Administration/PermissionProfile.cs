using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// Permission and RolePermission entity and domain model mapping configuration
    /// </summary>
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            // Permission: Entity -> Domain Model
            CreateMap<PermissionEntity, Permission>();

            // Permission: Domain Model -> Entity
            CreateMap<Permission, PermissionEntity>();

            // RolePermission: Entity -> Domain Model
            CreateMap<RolePermissionEntity, RolePermission>();

            // RolePermission: Domain Model -> Entity
            CreateMap<RolePermission, RolePermissionEntity>();
        }
    }
}
