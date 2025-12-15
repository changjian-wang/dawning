using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    public class ScopeProfile : Profile
    {
        public ScopeProfile()
        {
            CreateMap<Scope, ScopeDto>().ReverseMap();
        }
    }
}
