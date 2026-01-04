using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// Role entity and domain model mapper
    /// </summary>
    public static class RoleMappers
    {
        private static IMapper Mapper { get; }

        static RoleMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RoleProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static Role ToModel(this RoleEntity entity)
        {
            return Mapper.Map<Role>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static RoleEntity ToEntity(this Role model)
        {
            return Mapper.Map<RoleEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<Role> ToModels(this IEnumerable<RoleEntity> entities)
        {
            return Mapper.Map<IEnumerable<Role>>(entities);
        }

        /// <summary>
        /// Convert domain model collection to entity collection
        /// </summary>
        public static IEnumerable<RoleEntity> ToEntities(this IEnumerable<Role> models)
        {
            return Mapper.Map<IEnumerable<RoleEntity>>(models);
        }
    }
}
