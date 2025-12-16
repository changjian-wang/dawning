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
    /// 用户应用服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        Task<UserDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        Task<UserDto?> GetByUsernameAsync(string username);

        /// <summary>
        /// 获取分页用户列表
        /// </summary>
        Task<PagedData<UserDto>> GetPagedListAsync(UserModel model, int page, int itemsPerPage);

        /// <summary>
        /// 获取用户列表（Cursor 分页）
        /// </summary>
        Task<CursorPagedData<UserDto>> GetPagedListByCursorAsync(
            UserModel model,
            long? cursor,
            int pageSize
        );

        /// <summary>
        /// 创建用户
        /// </summary>
        Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId = null);

        /// <summary>
        /// 更新用户
        /// </summary>
        Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid? operatorId = null);

        /// <summary>
        /// 删除用户
        /// </summary>
        Task<bool> DeleteAsync(Guid id, Guid? operatorId = null);

        /// <summary>
        /// 修改密码
        /// </summary>
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

        /// <summary>
        /// 重置密码（管理员功能）
        /// </summary>
        Task<bool> ResetPasswordAsync(Guid userId, string newPassword);

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);

        /// <summary>
        /// 更新最后登录时间
        /// </summary>
        Task UpdateLastLoginAsync(Guid userId);

        /// <summary>
        /// 验证用户密码
        /// </summary>
        Task<UserDto?> ValidatePasswordAsync(string username, string password);

        /// <summary>
        /// 验证用户凭据并更新登录时间
        /// </summary>
        Task<UserDto?> ValidateCredentialsAndUpdateLoginAsync(string username, string password);

        /// <summary>
        /// 获取用户的所有角色
        /// </summary>
        Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// 获取用户详情（含角色）
        /// </summary>
        Task<UserWithRolesDto?> GetUserWithRolesAsync(Guid userId);

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        );

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// 获取用户统计信息
        /// </summary>
        Task<UserStatisticsDto> GetUserStatisticsAsync();

        /// <summary>
        /// 获取最近活跃用户（基于最后登录时间）
        /// </summary>
        Task<IEnumerable<RecentActiveUserDto>> GetRecentActiveUsersAsync(int count = 10);
    }

    /// <summary>
    /// 用户统计信息 DTO
    /// </summary>
    public class UserStatisticsDto
    {
        /// <summary>总用户数</summary>
        public int TotalUsers { get; set; }

        /// <summary>活跃用户数（is_active = true）</summary>
        public int ActiveUsers { get; set; }

        /// <summary>今日登录用户数</summary>
        public int TodayLoginUsers { get; set; }

        /// <summary>本周登录用户数</summary>
        public int WeekLoginUsers { get; set; }

        /// <summary>本月登录用户数</summary>
        public int MonthLoginUsers { get; set; }

        /// <summary>从未登录用户数</summary>
        public int NeverLoginUsers { get; set; }

        /// <summary>按角色统计</summary>
        public Dictionary<string, int> UsersByRole { get; set; } = new();

        /// <summary>统计时间</summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 最近活跃用户 DTO
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
