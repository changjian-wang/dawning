using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// Permission Aggregate Root
    /// </summary>
    public class Permission : IAggregateRoot
    {
        /// <summary>
        /// Permission unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Permission code (unique identifier, format: resource:action)
        /// Examples: user:create, role:update, audit-log:read
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Permission name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Permission description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Resource type (e.g., user, role, audit-log, etc.)
        /// </summary>
        public string Resource { get; set; } = string.Empty;

        /// <summary>
        /// Action type (e.g., create, read, update, delete, etc.)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Permission category (e.g., administration, system, business, etc.)
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Whether it is a system permission (system permissions cannot be deleted)
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

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
        /// Timestamp (for concurrency control)
        /// </summary>
        public long Timestamp { get; set; }
    }
}
