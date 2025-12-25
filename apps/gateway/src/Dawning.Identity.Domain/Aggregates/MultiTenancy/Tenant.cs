using System;

namespace Dawning.Identity.Domain.Aggregates.MultiTenancy
{
    /// <summary>
    /// 租户聚合根
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 租户代码（唯一标识，用于URL、Header等）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 租户名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 租户描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 绑定域名（可选，用于子域名识别租户）
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// 联系邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 租户Logo URL
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// 租户配置（JSON格式，存储自定义配置）
        /// </summary>
        public string? Settings { get; set; }

        /// <summary>
        /// 数据库连接字符串（可选，用于独立数据库隔离）
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 订阅计划（如 free, basic, pro, enterprise）
        /// </summary>
        public string Plan { get; set; } = "free";

        /// <summary>
        /// 订阅到期时间
        /// </summary>
        public DateTime? SubscriptionExpiresAt { get; set; }

        /// <summary>
        /// 最大用户数限制
        /// </summary>
        public int? MaxUsers { get; set; }

        /// <summary>
        /// 最大存储空间（MB）
        /// </summary>
        public int? MaxStorageMB { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

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
    }
}
