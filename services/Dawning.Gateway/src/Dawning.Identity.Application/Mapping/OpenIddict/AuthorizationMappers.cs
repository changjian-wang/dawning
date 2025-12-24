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
    public static class AuthorizationMappers
    {
        internal static IMapper Mapper { get; }

        static AuthorizationMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<AuthorizationProfile>()
            ).CreateMapper();
        }

        public static AuthorizationDto? ToDto(this Authorization model)
        {
            return model == null ? null : Mapper.Map<AuthorizationDto>(model);
        }

        public static IEnumerable<AuthorizationDto>? ToDtos(this IEnumerable<Authorization> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<AuthorizationDto>>(models);
        }

        public static Authorization? ToModel(this AuthorizationDto dto)
        {
            return dto == null ? null : Mapper.Map<Authorization>(dto);
        }
    }
}
