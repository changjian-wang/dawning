using Dawning.Identity.Domain.Core.Interfaces;
using System;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 权限聚合根
    /// </summary>
    public class Permission : IAggregateRoot
    {
        /// <summary>
        /// 权限唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 权限代码（唯一标识，格式：resource:action）
        /// 例如：user:create, role:update, audit-log:read
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 权限描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 资源类型（如：user, role, audit-log等）
        /// </summary>
        public string Resource { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型（如：create, read, update, delete等）
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 权限分类（如：administration, system, business等）
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 是否为系统权限（系统权限不可删除）
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 显示顺序
        /// </summary>
        public int DisplayOrder { get; set; }

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
        /// 时间戳（用于并发控制）
        /// </summary>
        public long Timestamp { get; set; }
    }
}
