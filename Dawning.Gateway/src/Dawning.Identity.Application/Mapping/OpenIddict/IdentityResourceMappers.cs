using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;

namespace Dawning.Identity.Application.Mapping.OpenIddict
{
    /// <summary>
    /// 身份资源映射器
    /// </summary>
    public static class IdentityResourceMappers
    {
        internal static IMapper Mapper { get; }

        static IdentityResourceMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<IdentityResourceProfile>()
            ).CreateMapper();
        }

        public static IdentityResourceDto? ToDto(this IdentityResource model)
        {
            return model == null ? null : Mapper.Map<IdentityResourceDto>(model);
        }

        public static IEnumerable<IdentityResourceDto>? ToDtos(
            this IEnumerable<IdentityResource> models
        )
        {
            return models == null ? null : Mapper.Map<IEnumerable<IdentityResourceDto>>(models);
        }

        public static IdentityResource? ToModel(this IdentityResourceDto dto)
        {
            return dto == null ? null : Mapper.Map<IdentityResource>(dto);
        }
    }

    /// <summary>
    /// 身份资源AutoMapper配置
    /// </summary>
    public class IdentityResourceProfile : Profile
    {
        public IdentityResourceProfile()
        {
            CreateMap<IdentityResource, IdentityResourceDto>();
            CreateMap<IdentityResourceDto, IdentityResource>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id ?? System.Guid.NewGuid())
                );
        }
    }
}
