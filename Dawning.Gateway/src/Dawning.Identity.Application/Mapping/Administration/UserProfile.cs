using AutoMapper;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// 用户DTO映射配置
    /// </summary>
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Domain Model -> DTO
            CreateMap<User, UserDto>();

            // DTO -> Domain Model
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore());
        }
    }
}
