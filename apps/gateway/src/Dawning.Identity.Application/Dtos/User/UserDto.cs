using System;

namespace Dawning.Identity.Application.Dtos.User
{
    /// <summary>
    /// User DTO (returned to client)
    /// </summary>
    public class UserDto
    {
        /// <summary>User unique identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Username (login name)</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Email</summary>
        public string? Email { get; set; }

        /// <summary>Phone number</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>Display name</summary>
        public string? DisplayName { get; set; }

        /// <summary>Avatar URL</summary>
        public string? Avatar { get; set; }

        /// <summary>Role</summary>
        public string Role { get; set; } = "user";

        /// <summary>Is active</summary>
        public bool IsActive { get; set; }

        /// <summary>Is system user (system users cannot be deleted/disabled)</summary>
        public bool IsSystem { get; set; }

        /// <summary>Is email confirmed</summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>Is phone number confirmed</summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>Last login time</summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Created time</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Updated time</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Remark</summary>
        public string? Remark { get; set; }

        /// <summary>Optimistic locking timestamp</summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Create User Request DTO
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>Username (login name)</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Password</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Email</summary>
        public string? Email { get; set; }

        /// <summary>Phone number</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>Display name</summary>
        public string? DisplayName { get; set; }

        /// <summary>Avatar URL</summary>
        public string? Avatar { get; set; }

        /// <summary>Role</summary>
        public string Role { get; set; } = "user";

        /// <summary>Is active</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Is system user (system users cannot be deleted/disabled, only used during system initialization)</summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>Remark</summary>
        public string? Remark { get; set; }
    }

    /// <summary>
    /// Update User Request DTO
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
    /// Change Password Request DTO
    /// </summary>
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reset Password Request (simplified, for API route parameters)
    /// </summary>
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
