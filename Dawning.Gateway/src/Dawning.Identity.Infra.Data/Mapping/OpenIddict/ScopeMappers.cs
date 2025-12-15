using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// Scope 映射器
    /// </summary>
    public static class ScopeMappers
    {
        private static IMapper Mapper { get; }

        static ScopeMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ScopeProfile>()).CreateMapper();
        }

        public static Scope ToModel(this ScopeEntity entity)
        {
            return Mapper.Map<Scope>(entity);
        }

        public static IEnumerable<Scope> ToModels(this IEnumerable<ScopeEntity> entities)
        {
            return Mapper.Map<IEnumerable<Scope>>(entities);
        }

        public static ScopeEntity ToEntity(this Scope model)
        {
            return Mapper.Map<ScopeEntity>(model);
        }
    }
}
