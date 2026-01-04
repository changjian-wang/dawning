using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// AuditLog entity and domain model mapper
    /// </summary>
    public static class AuditLogMappers
    {
        private static IMapper Mapper { get; }

        static AuditLogMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<AuditLogProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static AuditLog ToModel(this AuditLogEntity entity)
        {
            return Mapper.Map<AuditLog>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static AuditLogEntity ToEntity(this AuditLog model)
        {
            return Mapper.Map<AuditLogEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<AuditLog> ToModels(this IEnumerable<AuditLogEntity> entities)
        {
            return Mapper.Map<IEnumerable<AuditLog>>(entities);
        }

        /// <summary>
        /// Convert domain model collection to entity collection
        /// </summary>
        public static IEnumerable<AuditLogEntity> ToEntities(this IEnumerable<AuditLog> models)
        {
            return Mapper.Map<IEnumerable<AuditLogEntity>>(models);
        }
    }
}
