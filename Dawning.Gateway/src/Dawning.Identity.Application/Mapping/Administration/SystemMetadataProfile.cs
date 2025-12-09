using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    public class SystemMetadataProfile : Profile
    {
        public SystemMetadataProfile()
        {
            CreateMap<SystemMetadata, SystemMetadataDto>().ReverseMap();
        }
    }

    public static class SystemMetadataExtensions
    {
        public static SystemMetadataDto? ToDto(this SystemMetadata? model)
        {
            if (model == null) return null;

            return new SystemMetadataDto
            {
                Id = model.Id,
                Name = model.Name ?? string.Empty,
                Key = model.Key ?? string.Empty,
                Value = model.Value ?? string.Empty,
                Description = model.Description ?? string.Empty,
                NonEditable = model.NonEditable,
                Timestamp = model.Timestamp,
                Created = model.Created,
                Updated = model.Updated
            };
        }

        public static IEnumerable<SystemMetadataDto>? ToDtos(this IEnumerable<SystemMetadata>? models)
        {
            return models?.Select(m => m.ToDto()).Where(dto => dto != null).Cast<SystemMetadataDto>();
        }

        public static SystemMetadata ToEntity(this SystemMetadataDto dto)
        {
            return new SystemMetadata
            {
                Id = dto.Id ?? Guid.NewGuid(),
                Name = dto.Name,
                Key = dto.Key,
                Value = dto.Value,
                Description = dto.Description,
                NonEditable = dto.NonEditable
            };
        }
    }
}
