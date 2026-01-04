using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// AuditLog entity and domain model mapping configuration
    /// </summary>
    public class AuditLogProfile : Profile
    {
        public AuditLogProfile()
        {
            // Entity -> Domain Model
            CreateMap<AuditLogEntity, AuditLog>();

            // Domain Model -> Entity
            CreateMap<AuditLog, AuditLogEntity>();
        }
    }
}
