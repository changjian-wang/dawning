using System;

namespace Dawning.Identity.Application.Dtos.User
{
    /// <summary>
    /// 用户DTO（返回给客户端）
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = "user";
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Remark { get; set; }
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 创建用户请求DTO
    /// </summary>
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = "user";
        public bool IsActive { get; set; } = true;
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 更新用户请求DTO
    /// </summary>
    public class UpdateUserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 修改密码请求DTO
    /// </summary>
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 重置密码请求（简化版，用于API路由参数）
    /// </summary>
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
