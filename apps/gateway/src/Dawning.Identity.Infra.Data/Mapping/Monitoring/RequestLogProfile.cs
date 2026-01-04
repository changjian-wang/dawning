using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

namespace Dawning.Identity.Infra.Data.Mapping.Monitoring
{
    /// <summary>
    /// RequestLog entity and domain model mapping configuration
    /// </summary>
    public class RequestLogProfile : Profile
    {
        public RequestLogProfile()
        {
            // Entity -> Domain Model
            CreateMap<RequestLogEntity, RequestLog>();

            // Domain Model -> Entity
            CreateMap<RequestLog, RequestLogEntity>();
        }
    }
}
