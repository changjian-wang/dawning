using Dawning.Identity.Domain.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// 身份资源聚合根
    /// Represents user identity information (OpenID Connect)
    /// </summary>
    public class IdentityResource : IAggregateRoot
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 资源名称(唯一标识符)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 用户是否必须同意
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// 是否在同意界面中强调
        /// </summary>
        public bool Emphasize { get; set; } = false;

        /// <summary>
        /// 是否在发现文档中显示
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// 用户声明类型
        /// </summary>
        public List<string> UserClaims { get; set; } = new();

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
