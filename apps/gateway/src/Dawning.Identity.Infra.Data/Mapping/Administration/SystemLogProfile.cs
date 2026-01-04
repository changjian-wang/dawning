using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// SystemLog entity and domain model mapping configuration
    /// </summary>
    public class SystemLogProfile : Profile
    {
        public SystemLogProfile()
        {
            // Entity -> Domain Model
            CreateMap<SystemLogEntity, SystemLog>();

            // Domain Model -> Entity
            CreateMap<SystemLog, SystemLogEntity>();
        }
    }
}
