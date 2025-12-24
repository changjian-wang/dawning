using System;
using System.Collections.Generic;
using Dawning.Identity.Application.Dtos.User;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// 用户角色关联DTO
    /// </summary>
    public class UserRoleDto
    {
        /// <summary>
        /// 关联ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// 角色显示名称
        /// </summary>
        public string? RoleDisplayName { get; set; }

        /// <summary>
        /// 分配时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 分配者ID
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }

    /// <summary>
    /// 分配角色请求DTO
    /// </summary>
    public class AssignRolesDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 角色ID列表
        /// </summary>
        public List<Guid> RoleIds { get; set; } = new();
    }

    /// <summary>
    /// 用户详情（含角色）DTO
    /// </summary>
    public class UserWithRolesDto : UserDto
    {
        /// <summary>
        /// 用户的角色列表
        /// </summary>
        public List<RoleDto> Roles { get; set; } = new();
    }
}
