using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// Permission mapping profile
    /// </summary>
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            // Permission -> PermissionDto
            CreateMap<Permission, PermissionDto>();

            // CreatePermissionDto -> Permission
            CreateMap<CreatePermissionDto, Permission>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.IsSystem, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore());

            // UpdatePermissionDto -> Permission (for partial updates)
            CreateMap<UpdatePermissionDto, Permission>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }

    /// <summary>
    /// Permission mappers using AutoMapper
    /// </summary>
    public static class PermissionMappers
    {
        private static IMapper Mapper { get; }

        static PermissionMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<PermissionProfile>()
            ).CreateMapper();
        }

        /// <summary>
        /// Convert Permission to PermissionDto
        /// </summary>
        public static PermissionDto ToDto(this Permission model)
        {
            return Mapper.Map<PermissionDto>(model);
        }

        /// <summary>
        /// Convert Permission collection to PermissionDto collection
        /// </summary>
        public static IEnumerable<PermissionDto> ToDtos(this IEnumerable<Permission> models)
        {
            return Mapper.Map<IEnumerable<PermissionDto>>(models);
        }

        /// <summary>
        /// Convert CreatePermissionDto to Permission entity
        /// </summary>
        public static Permission ToEntity(this CreatePermissionDto dto)
        {
            return Mapper.Map<Permission>(dto);
        }

        /// <summary>
        /// Apply UpdatePermissionDto to existing Permission entity
        /// </summary>
        public static void ApplyUpdate(this Permission entity, UpdatePermissionDto dto)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
