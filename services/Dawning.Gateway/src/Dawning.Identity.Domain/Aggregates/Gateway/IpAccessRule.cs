using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// IP 访问规则聚合根
    /// </summary>
    public class IpAccessRule : IAggregateRoot
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// IP 地址或 CIDR 范围 (如 192.168.1.0/24)
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 规则类型: whitelist, blacklist
        /// </summary>
        public string RuleType { get; set; } = "blacklist";

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 过期时间（可选，用于临时封禁）
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public string? CreatedBy { get; set; }
    }
}
