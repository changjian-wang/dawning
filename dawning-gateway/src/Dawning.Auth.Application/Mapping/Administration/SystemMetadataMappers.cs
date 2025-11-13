using System;
using AutoMapper;
using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Domain.Aggregates.Administration;

namespace Dawning.Auth.Application.Mapping.Administration
{
	public static class SystemMetadataMappers
	{
        internal static IMapper Mapper { get; }

        static SystemMetadataMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SystemMetadataProfile>())
                .CreateMapper();
        }

        public static SystemMetadataDto? ToDto(this SystemMetadata model)
        {
            return model == null ? null : Mapper.Map<SystemMetadataDto>(model);
        }

        public static IEnumerable<SystemMetadataDto>? ToDtos(this IEnumerable<SystemMetadata> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<SystemMetadataDto>>(models);
        }

        public static SystemMetadata? ToModel(this SystemMetadataDto dto)
        {
            return dto == null ? null : Mapper.Map<SystemMetadata>(dto);
        }
    }
}

