using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<Domain.Aggregates.OpenIddict.Application, ApplicationDto>().ReverseMap();
        }
    }
}
