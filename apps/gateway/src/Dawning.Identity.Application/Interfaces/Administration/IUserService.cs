using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// User application service interface
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<UserDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get user by username
        /// </summary>
        Task<UserDto?> GetByUsernameAsync(string username);

        /// <summary>
        /// Get paged user list
        /// </summary>
        Task<PagedData<UserDto>> GetPagedListAsync(UserModel model, int page, int itemsPerPage);

        /// <summary>
        /// Get user list (Cursor pagination)
        /// </summary>
        Task<CursorPagedData<UserDto>> GetPagedListByCursorAsync(
            UserModel model,
            long? cursor,
            int pageSize
        );

        /// <summary>
        /// Create user
        /// </summary>
        Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId = null);

        /// <summary>
        /// Update user
        /// </summary>
        Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid? operatorId = null);

        /// <summary>
        /// Delete user
        /// </summary>
        Task<bool> DeleteAsync(Guid id, Guid? operatorId = null);

        /// <summary>
        /// Change password
        /// </summary>
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

        /// <summary>
        /// Reset password (admin function)
        /// </summary>
        Task<bool> ResetPasswordAsync(Guid userId, string newPassword);

        /// <summary>
        /// Check if username exists
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

        /// <summary>
        /// Check if email exists
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);

        /// <summary>
        /// Update last login time
        /// </summary>
        Task UpdateLastLoginAsync(Guid userId);

        /// <summary>
        /// Validate user password
        /// </summary>
        Task<UserDto?> ValidatePasswordAsync(string username, string password);

        /// <summary>
        /// Validate user credentials and update login time
        /// </summary>
        Task<UserDto?> ValidateCredentialsAndUpdateLoginAsync(string username, string password);

        /// <summary>
        /// Get all roles for user
        /// </summary>
        Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Get user details (with roles)
        /// </summary>
        Task<UserWithRolesDto?> GetUserWithRolesAsync(Guid userId);

        /// <summary>
        /// Assign roles to user
        /// </summary>
        Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        );

        /// <summary>
        /// Remove role from user
        /// </summary>
        Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// Get user statistics
        /// </summary>
        Task<UserStatisticsDto> GetUserStatisticsAsync();

        /// <summary>
        /// Get recently active users (based on last login time)
        /// </summary>
        Task<IEnumerable<RecentActiveUserDto>> GetRecentActiveUsersAsync(int count = 10);
    }

    /// <summary>
    /// User statistics DTO
    /// </summary>
    public class UserStatisticsDto
    {
        /// <summary>Total user count</summary>
        public int TotalUsers { get; set; }

        /// <summary>Active user count (is_active = true)</summary>
        public int ActiveUsers { get; set; }

        /// <summary>Users logged in today</summary>
        public int TodayLoginUsers { get; set; }

        /// <summary>Users logged in this week</summary>
        public int WeekLoginUsers { get; set; }

        /// <summary>Users logged in this month</summary>
        public int MonthLoginUsers { get; set; }

        /// <summary>Users who never logged in</summary>
        public int NeverLoginUsers { get; set; }

        /// <summary>Statistics by role</summary>
        public Dictionary<string, int> UsersByRole { get; set; } = new();

        /// <summary>Statistics generation time</summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Recently active user DTO
    /// </summary>
    public class RecentActiveUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
