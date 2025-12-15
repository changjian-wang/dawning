using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    public class SystemMetadataProfile : Profile
    {
        public SystemMetadataProfile()
        {
            CreateMap<SystemMetadataEntity, SystemMetadata>(MemberList.Destination).ReverseMap();
        }
    }
}
