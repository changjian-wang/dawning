using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// User persistent object (corresponds to database table)
    /// </summary>
    [Table("users")]
    public class UserEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password hash
        /// </summary>
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        [Column("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Avatar URL
        /// </summary>
        [Column("avatar")]
        public string? Avatar { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        [Column("role")]
        public string Role { get; set; } = "user";

        /// <summary>
        /// Whether active
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether system user (system users cannot be deleted/disabled)
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// Whether email is confirmed
        /// </summary>
        [Column("email_confirmed")]
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Whether phone number is confirmed
        /// </summary>
        [Column("phone_number_confirmed")]
        public bool PhoneNumberConfirmed { get; set; } = false;

        /// <summary>
        /// Last login time
        /// </summary>
        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Consecutive failed login attempts count
        /// </summary>
        [Column("failed_login_count")]
        public int FailedLoginCount { get; set; } = 0;

        /// <summary>
        /// Lockout end time
        /// </summary>
        [Column("lockout_end")]
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Whether lockout is enabled
        /// </summary>
        [Column("lockout_enabled")]
        public bool LockoutEnabled { get; set; } = true;

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Creator ID
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Updater ID
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        [Column("remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// Timestamp (used for pagination queries)
        /// </summary>
        [Column("timestamp")]
        public long Timestamp { get; set; }
    }
}
