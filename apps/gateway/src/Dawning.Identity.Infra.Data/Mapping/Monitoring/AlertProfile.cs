using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

namespace Dawning.Identity.Infra.Data.Mapping.Monitoring
{
    /// <summary>
    /// AlertRule and AlertHistory entity and domain model mapping configuration
    /// </summary>
    public class AlertProfile : Profile
    {
        public AlertProfile()
        {
            // AlertRule: Entity -> Domain Model
            CreateMap<AlertRuleEntity, AlertRule>();

            // AlertRule: Domain Model -> Entity
            CreateMap<AlertRule, AlertRuleEntity>();

            // AlertHistory: Entity -> Domain Model
            CreateMap<AlertHistoryEntity, AlertHistory>();

            // AlertHistory: Domain Model -> Entity
            CreateMap<AlertHistory, AlertHistoryEntity>();
        }
    }
}
