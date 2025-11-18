using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    public class AuthorizationProfile : Profile
    {
        public AuthorizationProfile()
        {
            CreateMap<Authorization, AuthorizationDto>().ReverseMap();
        }
    }
}
