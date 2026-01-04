using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// User Aggregate Root
    /// </summary>
    public class User : IAggregateRoot
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username (login name)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password hash
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Avatar URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Role (admin, user, manager, etc.)
        /// </summary>
        public string Role { get; set; } = "user";

        /// <summary>
        /// Whether active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether it is a system user (system users cannot be deleted/disabled)
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// Whether email is verified
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Whether phone number is verified
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; } = false;

        /// <summary>
        /// Last login time
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Consecutive failed login count
        /// </summary>
        public int FailedLoginCount { get; set; } = 0;

        /// <summary>
        /// Lockout end time
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Whether lockout is enabled
        /// </summary>
        public bool LockoutEnabled { get; set; } = true;

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Created by ID
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Updated by ID
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// Timestamp (for pagination queries)
        /// </summary>
        public long Timestamp { get; set; }
    }
}
