using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// SystemLog entity and domain model mapper
    /// </summary>
    public static class SystemLogMappers
    {
        private static IMapper Mapper { get; }

        static SystemLogMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SystemLogProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static SystemLog ToModel(this SystemLogEntity entity)
        {
            return Mapper.Map<SystemLog>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static SystemLogEntity ToEntity(this SystemLog model)
        {
            return Mapper.Map<SystemLogEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<SystemLog> ToModels(this IEnumerable<SystemLogEntity> entities)
        {
            return Mapper.Map<IEnumerable<SystemLog>>(entities);
        }

        /// <summary>
        /// Convert domain model collection to entity collection
        /// </summary>
        public static IEnumerable<SystemLogEntity> ToEntities(this IEnumerable<SystemLog> models)
        {
            return Mapper.Map<IEnumerable<SystemLogEntity>>(models);
        }
    }
}
