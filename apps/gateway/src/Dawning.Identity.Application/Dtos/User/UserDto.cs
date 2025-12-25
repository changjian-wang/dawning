using System;

namespace Dawning.Identity.Application.Dtos.User
{
    /// <summary>
    /// 用户DTO（返回给客户端）
    /// </summary>
    public class UserDto
    {
        /// <summary>用户唯一标识</summary>
        public Guid Id { get; set; }

        /// <summary>用户名（登录名）</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>邮箱</summary>
        public string? Email { get; set; }

        /// <summary>手机号</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>显示名称</summary>
        public string? DisplayName { get; set; }

        /// <summary>头像URL</summary>
        public string? Avatar { get; set; }

        /// <summary>角色</summary>
        public string Role { get; set; } = "user";

        /// <summary>是否激活</summary>
        public bool IsActive { get; set; }

        /// <summary>是否为系统用户（系统用户不可删除/禁用）</summary>
        public bool IsSystem { get; set; }

        /// <summary>邮箱是否已验证</summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>手机号是否已验证</summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>最后登录时间</summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>更新时间</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>备注</summary>
        public string? Remark { get; set; }

        /// <summary>乐观锁时间戳</summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 创建用户请求DTO
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>用户名（登录名）</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>密码</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>邮箱</summary>
        public string? Email { get; set; }

        /// <summary>手机号</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>显示名称</summary>
        public string? DisplayName { get; set; }

        /// <summary>头像URL</summary>
        public string? Avatar { get; set; }

        /// <summary>角色</summary>
        public string Role { get; set; } = "user";

        /// <summary>是否激活</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>是否为系统用户（系统用户不可删除/禁用，仅系统初始化时使用）</summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>备注</summary>
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
