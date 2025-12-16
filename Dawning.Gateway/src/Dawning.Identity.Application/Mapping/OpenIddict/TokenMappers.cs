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
    public static class TokenMappers
    {
        internal static IMapper Mapper { get; }

        static TokenMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<TokenProfile>()).CreateMapper();
        }

        public static TokenDto? ToDto(this TokenEntity model)
        {
            return model == null ? null : Mapper.Map<TokenDto>(model);
        }

        public static IEnumerable<TokenDto>? ToDtos(this IEnumerable<TokenEntity> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<TokenDto>>(models);
        }

        public static TokenEntity? ToModel(this TokenDto dto)
        {
            return dto == null ? null : Mapper.Map<TokenEntity>(dto);
        }
    }
}
