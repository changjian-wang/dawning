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
    public static class TokenMappers
    {
        internal static IMapper Mapper { get; }

        static TokenMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<TokenProfile>())
                .CreateMapper();
        }

        public static TokenDto? ToDto(this Token model)
        {
            return model == null ? null : Mapper.Map<TokenDto>(model);
        }

        public static IEnumerable<TokenDto>? ToDtos(this IEnumerable<Token> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<TokenDto>>(models);
        }

        public static Token? ToModel(this TokenDto dto)
        {
            return dto == null ? null : Mapper.Map<Token>(dto);
        }
    }
}
