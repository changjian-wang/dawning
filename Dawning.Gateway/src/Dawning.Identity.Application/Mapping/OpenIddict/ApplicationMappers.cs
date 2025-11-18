using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    public static class ApplicationMappers
    {
        internal static IMapper Mapper { get; }

        static ApplicationMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationProfile>())
                .CreateMapper();
        }

        public static ApplicationDto? ToDto(this Domain.Aggregates.OpenIddict.Application model)
        {
            return model == null ? null : Mapper.Map<ApplicationDto>(model);
        }

        public static IEnumerable<ApplicationDto>? ToDtos(this IEnumerable<Domain.Aggregates.OpenIddict.Application> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<ApplicationDto>>(models);
        }

        public static Domain.Aggregates.OpenIddict.Application? ToModel(this ApplicationDto dto)
        {
            return dto == null ? null : Mapper.Map<Domain.Aggregates.OpenIddict.Application>(dto);
        }
    }
}
