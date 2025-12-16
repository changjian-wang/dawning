using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using TokenEntity = Dawning.Identity.Domain.Aggregates.OpenIddict.Token;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    public class TokenProfile : Profile
    {
        public TokenProfile()
        {
            CreateMap<TokenEntity, TokenDto>().ReverseMap();
        }
    }
}
