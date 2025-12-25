using System;
using System.Collections.Generic;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// 权限DTO
    /// </summary>
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 创建权限DTO
    /// </summary>
    public class CreatePermissionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// 更新权限DTO
    /// </summary>
    public class UpdatePermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// 角色权限DTO
    /// </summary>
    public class RolePermissionDto
    {
        public Guid RoleId { get; set; }
        public List<Guid> PermissionIds { get; set; } = new();
    }

    /// <summary>
    /// 权限分组DTO（按资源分组）
    /// </summary>
    public class PermissionGroupDto
    {
        public string Resource { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
