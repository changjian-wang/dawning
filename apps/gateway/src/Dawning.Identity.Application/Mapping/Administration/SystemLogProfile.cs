using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// System log AutoMapper configuration
    /// </summary>
    public class SystemLogProfile : Profile
    {
        public SystemLogProfile()
        {
            // SystemLog <-> SystemLogDto
            CreateMap<SystemLog, SystemLogDto>();
            CreateMap<SystemLogDto, SystemLog>();

            // CreateSystemLogDto -> SystemLog
            CreateMap<CreateSystemLogDto, SystemLog>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }

    /// <summary>
    /// SystemLog static mappers using AutoMapper
    /// </summary>
    public static class SystemLogMappers
    {
        private static IMapper Mapper { get; }

        static SystemLogMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SystemLogProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert SystemLog to SystemLogDto
        /// </summary>
        public static SystemLogDto ToDto(this SystemLog log) => Mapper.Map<SystemLogDto>(log);

        /// <summary>
        /// Convert SystemLog to SystemLogDto (nullable)
        /// </summary>
        public static SystemLogDto? ToDtoOrNull(this SystemLog? log) =>
            log != null ? Mapper.Map<SystemLogDto>(log) : null;

        /// <summary>
        /// Convert SystemLog collection to SystemLogDto collection
        /// </summary>
        public static IEnumerable<SystemLogDto> ToDtos(this IEnumerable<SystemLog> logs) =>
            logs.Select(l => l.ToDto());

        /// <summary>
        /// Convert SystemLog array to SystemLogDto array
        /// </summary>
        public static SystemLogDto[] ToDtoArray(this IEnumerable<SystemLog> logs) =>
            logs.Select(l => l.ToDto()).ToArray();

        /// <summary>
        /// Convert CreateSystemLogDto to SystemLog entity
        /// </summary>
        public static SystemLog ToEntity(this CreateSystemLogDto dto)
        {
            var entity = Mapper.Map<SystemLog>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }
    }
}
