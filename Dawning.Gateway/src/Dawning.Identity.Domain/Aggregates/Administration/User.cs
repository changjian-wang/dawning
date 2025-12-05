using Dawning.Identity.Domain.Core.Interfaces;
using System;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 用户聚合根
    /// </summary>
    public class User : IAggregateRoot
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 用户名（登录名）
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 角色（admin, user, manager等）
        /// </summary>
        public string Role { get; set; } = "user";

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 是否已删除（软删除）
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 邮箱是否已验证
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// 手机号是否已验证
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; } = false;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 时间戳（用于分页查询）
        /// </summary>
        public long Timestamp { get; set; }
    }
}
