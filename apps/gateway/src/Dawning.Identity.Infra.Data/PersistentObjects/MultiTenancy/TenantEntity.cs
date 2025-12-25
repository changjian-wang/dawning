using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.MultiTenancy
{
    /// <summary>
    /// 租户数据库实体
    /// </summary>
    [Table("tenants")]
    public class TenantEntity
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 租户代码（唯一标识）
        /// </summary>
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 租户名称
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 租户描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 绑定域名
        /// </summary>
        [Column("domain")]
        public string? Domain { get; set; }

        /// <summary>
        /// 联系邮箱
        /// </summary>
        [Column("email")]
        public string? Email { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [Column("phone")]
        public string? Phone { get; set; }

        /// <summary>
        /// 租户Logo URL
        /// </summary>
        [Column("logo_url")]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// 租户配置（JSON）
        /// </summary>
        [Column("settings")]
        public string? Settings { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        [Column("connection_string")]
        public string? ConnectionString { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 订阅计划
        /// </summary>
        [Column("plan")]
        public string Plan { get; set; } = "free";

        /// <summary>
        /// 订阅到期时间
        /// </summary>
        [Column("subscription_expires_at")]
        public DateTime? SubscriptionExpiresAt { get; set; }

        /// <summary>
        /// 最大用户数限制
        /// </summary>
        [Column("max_users")]
        public int? MaxUsers { get; set; }

        /// <summary>
        /// 最大存储空间（MB）
        /// </summary>
        [Column("max_storage_mb")]
        public int? MaxStorageMB { get; set; }

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
    }
}
