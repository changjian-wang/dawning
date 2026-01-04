using System;
using System.Collections.Generic;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// Role Aggregate Root
    /// </summary>
    public class Role : IAggregateRoot
    {
        /// <summary>
        /// Role unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Role name (unique identifier, e.g., admin, user, manager)
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
        /// Whether it is a system role (system roles cannot be deleted)
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Role permissions list (JSON array string)
        /// Format: ["resource:action:scope", ...]
        /// Examples: ["user:create:*", "user:read:own"]
        /// </summary>
        public string? Permissions { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Created by ID
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updated by ID
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Soft delete time
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; }
    }
}
