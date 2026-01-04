using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// Permission and RolePermission entity and domain model mapper
    /// </summary>
    public static class PermissionMappers
    {
        private static IMapper Mapper { get; }

        static PermissionMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<PermissionProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static Permission ToDomain(this PermissionEntity entity)
        {
            return Mapper.Map<Permission>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static PermissionEntity ToEntity(this Permission model)
        {
            return Mapper.Map<PermissionEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static List<Permission>? ToDomains(this IEnumerable<PermissionEntity>? entities)
        {
            return entities?.Select(e => e.ToDomain()).ToList();
        }

        /// <summary>
        /// Convert RolePermission entity to domain model
        /// </summary>
        public static RolePermission ToDomain(this RolePermissionEntity entity)
        {
            return Mapper.Map<RolePermission>(entity);
        }

        /// <summary>
        /// Convert RolePermission domain model to entity
        /// </summary>
        public static RolePermissionEntity ToEntity(this RolePermission model)
        {
            return Mapper.Map<RolePermissionEntity>(model);
        }
    }
}
