using System;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// BackupRecord entity and domain model mapping configuration
    /// </summary>
    public class BackupRecordProfile : Profile
    {
        public BackupRecordProfile()
        {
            // Entity -> Domain Model (handle Guid parsing)
            CreateMap<BackupRecordEntity, BackupRecord>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ParseGuid(src.Id)));

            // Domain Model -> Entity (handle Guid to string)
            CreateMap<BackupRecord, BackupRecordEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }

        private static Guid ParseGuid(string? value)
        {
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
