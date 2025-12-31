using System;
using System.Collections.Generic;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// Role DTO
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// Role ID
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Role name (unique identifier)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Role display name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Role description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether it is a system role
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Whether it is enabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Permission list
        /// </summary>
        public List<string>? Permissions { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Creator ID
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updater ID
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Create role DTO
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// Role name (unique identifier)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Role display name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Role description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether it is enabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Permission list
        /// </summary>
        public List<string>? Permissions { get; set; }
    }

    /// <summary>
    /// Update role DTO
    /// </summary>
    public class UpdateRoleDto
    {
        /// <summary>
        /// Role ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Role display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Role description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether it is enabled
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Permission list
        /// </summary>
        public List<string>? Permissions { get; set; }
    }
}
