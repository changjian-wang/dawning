using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// 角色映射Profile
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
                .ForMember(dest => dest.IsSystem, opt => opt.MapFrom(src => false)); // 新创建的角色不是系统角色

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
}
