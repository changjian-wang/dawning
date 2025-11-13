using System;
using AutoMapper;
using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Domain.Aggregates.Administration;

namespace Dawning.Auth.Application.Mapping.Administration
{
    public static class ClaimTypeMappers
    {
        internal static IMapper Mapper { get; }

        static ClaimTypeMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ClaimTypeProfile>())
                .CreateMapper();
        }

        public static ClaimTypeDto? ToDto(this ClaimType model)
        {
            return model == null ? null : Mapper.Map<ClaimTypeDto>(model);
        }

        public static IEnumerable<ClaimTypeDto>? ToDtos(this IEnumerable<ClaimType> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<ClaimTypeDto>>(models);
        }

        public static ClaimType? ToModel(this ClaimTypeDto dto)
        {
            return dto == null ? null : Mapper.Map<ClaimType>(dto);
        }
    }
}

