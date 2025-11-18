using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// Token 映射器
    /// </summary>
    public static class TokenMappers
    {
        private static IMapper Mapper { get; }

        static TokenMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<TokenProfile>())
                .CreateMapper();
        }

        public static Token ToModel(this TokenEntity entity)
        {
            return Mapper.Map<Token>(entity);
        }

        public static IEnumerable<Token> ToModels(this IEnumerable<TokenEntity> entities)
        {
            return Mapper.Map<IEnumerable<Token>>(entities);
        }

        public static TokenEntity ToEntity(this Token model)
        {
            return Mapper.Map<TokenEntity>(model);
        }
    }
}
