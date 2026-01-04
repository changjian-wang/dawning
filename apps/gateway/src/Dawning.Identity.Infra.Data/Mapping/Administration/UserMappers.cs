using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// User entity and domain model mapper
    /// </summary>
    public static class UserMappers
    {
        private static IMapper Mapper { get; }

        static UserMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert user entity to domain model
        /// </summary>
        public static User ToModel(this UserEntity entity)
        {
            return Mapper.Map<User>(entity);
        }

        /// <summary>
        /// Convert user entity collection to domain model collection
        /// </summary>
        public static IEnumerable<User> ToModels(this IEnumerable<UserEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// Convert domain model to user entity
        /// </summary>
        public static UserEntity ToEntity(this User model)
        {
            return Mapper.Map<UserEntity>(model);
        }
    }
}
