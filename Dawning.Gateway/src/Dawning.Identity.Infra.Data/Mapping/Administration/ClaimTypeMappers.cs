using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    public static class ClaimTypeMappers
    {
        private static IMapper Mapper { get; }

        static ClaimTypeMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<ClaimTypeProfile>()
            ).CreateMapper();
        }

        public static ClaimType ToModel(this ClaimTypeEntity entity)
        {
            return Mapper.Map<ClaimType>(entity);
        }

        public static IEnumerable<ClaimType> ToModels(this IEnumerable<ClaimTypeEntity> entities)
        {
            return Mapper.Map<IEnumerable<ClaimType>>(entities);
        }

        public static ClaimTypeEntity ToEntity(this ClaimType model)
        {
            return Mapper.Map<ClaimTypeEntity>(model);
        }
    }
}
