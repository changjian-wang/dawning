using System;
using AutoMapper;
using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Auth.Infra.Data.Mapping.Administration
{
	public static class ClaimTypeMappers
	{
        private static IMapper Mapper { get; }

        static ClaimTypeMappers()
		{
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ClaimTypeProfile>())
                .CreateMapper();
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

