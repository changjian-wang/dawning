using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.MultiTenancy
{
    /// <summary>
    /// Tenant database entity
    /// </summary>
    [Table("tenants")]
    public class TenantEntity
    {
        /// <summary>
        /// Tenant ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Tenant code (unique identifier)
        /// </summary>
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tenant name
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tenant description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Bound domain
        /// </summary>
        [Column("domain")]
        public string? Domain { get; set; }

        /// <summary>
        /// Contact email
        /// </summary>
        [Column("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        [Column("phone")]
        public string? Phone { get; set; }

        /// <summary>
        /// Tenant Logo URL
        /// </summary>
        [Column("logo_url")]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Tenant settings (JSON)
        /// </summary>
        [Column("settings")]
        public string? Settings { get; set; }

        /// <summary>
        /// Database connection string
        /// </summary>
        [Column("connection_string")]
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Subscription plan
        /// </summary>
        [Column("plan")]
        public string Plan { get; set; } = "free";

        /// <summary>
        /// Subscription expiration time
        /// </summary>
        [Column("subscription_expires_at")]
        public DateTime? SubscriptionExpiresAt { get; set; }

        /// <summary>
        /// Maximum user limit
        /// </summary>
        [Column("max_users")]
        public int? MaxUsers { get; set; }

        /// <summary>
        /// Maximum storage space (MB)
        /// </summary>
        [Column("max_storage_mb")]
        public int? MaxStorageMB { get; set; }

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
    }
}
