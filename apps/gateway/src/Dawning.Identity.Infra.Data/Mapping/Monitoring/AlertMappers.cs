using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

namespace Dawning.Identity.Infra.Data.Mapping.Monitoring
{
    /// <summary>
    /// AlertRule entity and domain model mapper
    /// </summary>
    public static class AlertRuleMappers
    {
        private static IMapper Mapper { get; }

        static AlertRuleMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<AlertProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static AlertRule ToModel(this AlertRuleEntity entity)
        {
            return Mapper.Map<AlertRule>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static AlertRuleEntity ToEntity(this AlertRule model)
        {
            return Mapper.Map<AlertRuleEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<AlertRule> ToModels(this IEnumerable<AlertRuleEntity> entities)
        {
            return Mapper.Map<IEnumerable<AlertRule>>(entities);
        }
    }

    /// <summary>
    /// AlertHistory entity and domain model mapper
    /// </summary>
    public static class AlertHistoryMappers
    {
        private static IMapper Mapper { get; }

        static AlertHistoryMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<AlertProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static AlertHistory ToModel(this AlertHistoryEntity entity)
        {
            return Mapper.Map<AlertHistory>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static AlertHistoryEntity ToEntity(this AlertHistory model)
        {
            return Mapper.Map<AlertHistoryEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<AlertHistory> ToModels(this IEnumerable<AlertHistoryEntity> entities)
        {
            return Mapper.Map<IEnumerable<AlertHistory>>(entities);
        }
    }
}
