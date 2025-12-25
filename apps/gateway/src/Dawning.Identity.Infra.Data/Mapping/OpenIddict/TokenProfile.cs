using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    public class TokenProfile : Profile
    {
        public TokenProfile()
        {
            CreateMap<TokenEntity, Token>(MemberList.Destination).ReverseMap();
        }
    }
}
