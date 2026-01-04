using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// BackupRecord entity and domain model mapper
    /// </summary>
    public static class BackupRecordMappers
    {
        private static IMapper Mapper { get; }

        static BackupRecordMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<BackupRecordProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static BackupRecord ToModel(this BackupRecordEntity entity)
        {
            return Mapper.Map<BackupRecord>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static BackupRecordEntity ToEntity(this BackupRecord model)
        {
            return Mapper.Map<BackupRecordEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<BackupRecord> ToModels(this IEnumerable<BackupRecordEntity> entities)
        {
            return Mapper.Map<IEnumerable<BackupRecord>>(entities);
        }
    }
}
