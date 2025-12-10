using Dawning.Shared.Dapper.Contrib;
using System;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 权限持久化对象
    /// </summary>
    [Table("permissions")]
    public class PermissionEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("resource")]
        public string Resource { get; set; } = string.Empty;

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("category")]
        public string? Category { get; set; }

        [Column("is_system")]
        public bool IsSystem { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("timestamp")]
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 角色权限关联持久化对象
    /// </summary>
    [Table("role_permissions")]
    public class RolePermissionEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("permission_id")]
        public Guid PermissionId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}
