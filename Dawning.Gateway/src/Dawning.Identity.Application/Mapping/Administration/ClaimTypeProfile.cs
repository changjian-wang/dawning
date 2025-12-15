using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    public class ClaimTypeProfile : Profile
    {
        public ClaimTypeProfile()
        {
            CreateMap<ClaimType, ClaimTypeDto>().ReverseMap();
        }
    }

    public static class ClaimTypeExtensions
    {
        public static ClaimTypeDto? ToDto(this ClaimType? model)
        {
            if (model == null)
                return null;

            return new ClaimTypeDto
            {
                Id = model.Id,
                Name = model.Name ?? string.Empty,
                DisplayName = model.DisplayName ?? string.Empty,
                Type = model.Type ?? string.Empty,
                Description = model.Description ?? string.Empty,
                Required = model.Required,
                NonEditable = model.NonEditable,
                Timestamp = model.Timestamp,
            };
        }

        public static IEnumerable<ClaimTypeDto>? ToDtos(this IEnumerable<ClaimType>? models)
        {
            return models?.Select(m => m.ToDto()).Where(dto => dto != null).Cast<ClaimTypeDto>();
        }

        public static ClaimType ToEntity(this ClaimTypeDto dto)
        {
            return new ClaimType
            {
                Id = dto.Id ?? Guid.NewGuid(),
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                Type = dto.Type,
                Description = dto.Description,
                Required = dto.Required,
                NonEditable = dto.NonEditable,
            };
        }
    }
}
