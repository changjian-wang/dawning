using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// User mappers using AutoMapper
    /// </summary>
    public static class UserMappers
    {
        private static IMapper Mapper { get; }

        static UserMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert User to UserDto
        /// </summary>
        public static UserDto ToDto(this User model) => Mapper.Map<UserDto>(model);

        /// <summary>
        /// Convert User to UserDto (nullable)
        /// </summary>
        public static UserDto? ToDtoOrNull(this User? model) =>
            model != null ? Mapper.Map<UserDto>(model) : null;

        /// <summary>
        /// Convert User collection to UserDto collection
        /// </summary>
        public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> models) =>
            models.Select(m => m.ToDto());

        /// <summary>
        /// Convert CreateUserDto to User entity
        /// </summary>
        public static User ToEntity(this CreateUserDto dto)
        {
            var entity = Mapper.Map<User>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        /// <summary>
        /// Apply UpdateUserDto to existing User entity
        /// </summary>
        public static void ApplyUpdate(this User user, UpdateUserDto dto)
        {
            if (dto.Email != null)
                user.Email = dto.Email;
            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;
            if (dto.DisplayName != null)
                user.DisplayName = dto.DisplayName;
            if (dto.Avatar != null)
                user.Avatar = dto.Avatar;
            if (dto.Role != null)
                user.Role = dto.Role;
            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;
            if (dto.Remark != null)
                user.Remark = dto.Remark;
        }

        /// <summary>
        /// Convert UserDto to UserWithRolesDto
        /// </summary>
        public static UserWithRolesDto ToUserWithRolesDto(this UserDto dto) =>
            Mapper.Map<UserWithRolesDto>(dto);
    }
}
