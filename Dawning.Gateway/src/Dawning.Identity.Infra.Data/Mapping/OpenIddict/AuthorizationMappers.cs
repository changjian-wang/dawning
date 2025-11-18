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
    /// Authorization 映射器
    /// </summary>
    public static class AuthorizationMappers
    {
        private static IMapper Mapper { get; }

        static AuthorizationMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<AuthorizationProfile>())
                .CreateMapper();
        }

        public static Authorization ToModel(this AuthorizationEntity entity)
        {
            return Mapper.Map<Authorization>(entity);
        }

        public static IEnumerable<Authorization> ToModels(this IEnumerable<AuthorizationEntity> entities)
        {
            return Mapper.Map<IEnumerable<Authorization>>(entities);
        }

        public static AuthorizationEntity ToEntity(this Authorization model)
        {
            return Mapper.Map<AuthorizationEntity>(model);
        }
    }
}
