using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using System.Collections.Generic;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    /// <summary>
    /// API资源映射器
    /// </summary>
    public static class ApiResourceMappers
    {
        internal static IMapper Mapper { get; }

        static ApiResourceMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceProfile>())
                .CreateMapper();
        }

        public static ApiResourceDto? ToDto(this ApiResource model)
        {
            return model == null ? null : Mapper.Map<ApiResourceDto>(model);
        }

        public static IEnumerable<ApiResourceDto>? ToDtos(this IEnumerable<ApiResource> models)
        {
            return models == null ? null : Mapper.Map<IEnumerable<ApiResourceDto>>(models);
        }

        public static ApiResource? ToModel(this ApiResourceDto dto)
        {
            return dto == null ? null : Mapper.Map<ApiResource>(dto);
        }
    }

    /// <summary>
    /// API资源AutoMapper配置
    /// </summary>
    public class ApiResourceProfile : Profile
    {
        public ApiResourceProfile()
        {
            CreateMap<ApiResource, ApiResourceDto>();
            CreateMap<ApiResourceDto, ApiResource>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? System.Guid.NewGuid()));
        }
    }
}
