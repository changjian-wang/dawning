using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    public class ClaimTypeProfile : Profile
    {
        public ClaimTypeProfile()
        {
            CreateMap<ClaimTypeEntity, ClaimType>(MemberList.Destination).ReverseMap();
        }
    }
}
