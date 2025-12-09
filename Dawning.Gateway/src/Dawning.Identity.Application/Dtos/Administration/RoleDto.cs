using System;
using System.Collections.Generic;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// 角色DTO
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// 角色名称（唯一标识）
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
        /// 是否为系统角色
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<string>? Permissions { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedAt { get; set; }

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
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 创建角色DTO
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// 角色名称（唯一标识）
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
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<string>? Permissions { get; set; }
    }

    /// <summary>
    /// 更新角色DTO
    /// </summary>
    public class UpdateRoleDto
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 角色显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<string>? Permissions { get; set; }
    }
}
