using System;
using System.Collections.Generic;
using Dawning.Identity.Application.Dtos.User;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// User Role Association DTO
    /// </summary>
    public class UserRoleDto
    {
        /// <summary>
        /// Association ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Role Name
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Role Display Name
        /// </summary>
        public string? RoleDisplayName { get; set; }

        /// <summary>
        /// Assignment Time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Assigner ID
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }

    /// <summary>
    /// Assign Roles Request DTO
    /// </summary>
    public class AssignRolesDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Role ID List
        /// </summary>
        public List<Guid> RoleIds { get; set; } = new();
    }

    /// <summary>
    /// User Details (with Roles) DTO
    /// </summary>
    public class UserWithRolesDto : UserDto
    {
        /// <summary>
        /// User's Role List
        /// </summary>
        public List<RoleDto> Roles { get; set; } = new();
    }
}
