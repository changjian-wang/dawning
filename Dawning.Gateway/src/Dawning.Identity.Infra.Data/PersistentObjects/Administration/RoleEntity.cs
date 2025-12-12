using Dawning.Shared.Dapper.Contrib;
using System;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 角色数据库实体
    /// </summary>
    [Table("roles")]
    public class RoleEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否为系统角色
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// 权限列表（JSON字符串）
        /// </summary>
        [Column("permissions")]
        public string? Permissions { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// 软删除时间
        /// </summary>
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 时间戳（计算字段，不映射到数据库）
        /// </summary>
        [Computed]
        public long Timestamp { get; set; }
    }
}
