using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// Role mapping profile
    /// </summary>
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            // Role -> RoleDto
            CreateMap<Role, RoleDto>()
                .ForMember(
                    dest => dest.Permissions,
                    opt => opt.MapFrom(src => DeserializePermissions(src.Permissions))
                );

            // CreateRoleDto -> Role
            CreateMap<CreateRoleDto, Role>()
                .ForMember(
                    dest => dest.Permissions,
                    opt => opt.MapFrom(src => SerializePermissions(src.Permissions))
                )
                .ForMember(dest => dest.IsSystem, opt => opt.MapFrom(src => false)); // Newly created roles are not system roles

            // UpdateRoleDto -> Role
            CreateMap<UpdateRoleDto, Role>()
                .ForMember(
                    dest => dest.Permissions,
                    opt => opt.MapFrom(src => SerializePermissions(src.Permissions))
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        private static List<string> DeserializePermissions(string? permissions)
        {
            if (string.IsNullOrWhiteSpace(permissions))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(permissions) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string? SerializePermissions(List<string>? permissions)
        {
            if (permissions == null || !permissions.Any())
                return null;

            return JsonSerializer.Serialize(permissions);
        }
    }

    /// <summary>
    /// Role static mappers using AutoMapper
    /// </summary>
    public static class RoleMappers
    {
        private static IMapper Mapper { get; }

        static RoleMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RoleProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert Role to RoleDto
        /// </summary>
        public static RoleDto ToDto(this Role role) => Mapper.Map<RoleDto>(role);

        /// <summary>
        /// Convert Role collection to RoleDto collection
        /// </summary>
        public static IEnumerable<RoleDto> ToDtos(this IEnumerable<Role> roles) =>
            roles.Select(r => r.ToDto());

        /// <summary>
        /// Convert CreateRoleDto to Role entity
        /// </summary>
        public static Role ToEntity(this CreateRoleDto dto)
        {
            var entity = Mapper.Map<Role>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        /// <summary>
        /// Apply UpdateRoleDto to existing Role entity
        /// </summary>
        public static void ApplyUpdate(this Role role, UpdateRoleDto dto)
        {
            if (dto.DisplayName != null)
                role.DisplayName = dto.DisplayName;
            if (dto.Description != null)
                role.Description = dto.Description;
            if (dto.IsActive.HasValue)
                role.IsActive = dto.IsActive.Value;
            if (dto.Permissions != null)
            {
                role.Permissions = dto.Permissions.Any()
                    ? JsonSerializer.Serialize(dto.Permissions)
                    : null;
            }
        }
    }
}
