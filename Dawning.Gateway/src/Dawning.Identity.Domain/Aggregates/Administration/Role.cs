using Dawning.Identity.Domain.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 角色聚合根
    /// </summary>
    public class Role : IAggregateRoot
    {
        /// <summary>
        /// 角色唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 角色名称（唯一标识，如admin、user、manager）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否为系统角色（系统角色不可删除）
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 角色权限列表（JSON数组字符串）
        /// 格式: ["resource:action:scope", ...]
        /// 例如: ["user:create:*", "user:read:own"]
        /// </summary>
        public string? Permissions { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// 软删除时间
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }
}
