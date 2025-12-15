using System;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 用户持久化对象（对应数据库表）
    /// </summary>
    [Table("users")]
    public class UserEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希
        /// </summary>
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("email")]
        public string? Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        [Column("avatar")]
        public string? Avatar { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [Column("role")]
        public string Role { get; set; } = "user";

        /// <summary>
        /// 是否激活
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 邮箱是否已验证
        /// </summary>
        [Column("email_confirmed")]
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// 手机号是否已验证
        /// </summary>
        [Column("phone_number_confirmed")]
        public bool PhoneNumberConfirmed { get; set; } = false;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 时间戳（用于分页查询）
        /// </summary>
        [Column("timestamp")]
        public long Timestamp { get; set; }
    }
}
