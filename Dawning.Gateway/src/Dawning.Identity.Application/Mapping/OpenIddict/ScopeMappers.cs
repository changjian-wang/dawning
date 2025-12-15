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
    public static class ScopeMappers
    {
        internal static IMapper Mapper { get; }

        static ScopeMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ScopeProfile>()).CreateMapper();
        }

        public static ScopeDto? ToDto(this Scope model)
        {
            return model == null ? null : Mapper.Map<ScopeDto>(model);
        }

        public static IEnumerable<ScopeDto>? ToDtos(this IEnumerable<Scope> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<ScopeDto>>(models);
        }

        public static Scope? ToModel(this ScopeDto dto)
        {
            return dto == null ? null : Mapper.Map<Scope>(dto);
        }
    }
}
