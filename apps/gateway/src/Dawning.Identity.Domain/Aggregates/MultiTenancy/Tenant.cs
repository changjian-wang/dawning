using System;

namespace Dawning.Identity.Domain.Aggregates.MultiTenancy
{
    /// <summary>
    /// Tenant aggregate root
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// Tenant ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tenant code (unique identifier, used for URL, Header, etc.)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tenant name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tenant description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Bound domain (optional, used for subdomain tenant identification)
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// Contact email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Tenant logo URL
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Tenant settings (JSON format, stores custom configuration)
        /// </summary>
        public string? Settings { get; set; }

        /// <summary>
        /// Database connection string (optional, for isolated database separation)
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Subscription plan (e.g., free, basic, pro, enterprise)
        /// </summary>
        public string Plan { get; set; } = "free";

        /// <summary>
        /// Subscription expiration time
        /// </summary>
        public DateTime? SubscriptionExpiresAt { get; set; }

        /// <summary>
        /// Maximum user count limit
        /// </summary>
        public int? MaxUsers { get; set; }

        /// <summary>
        /// Maximum storage space (MB)
        /// </summary>
        public int? MaxStorageMB { get; set; }

        /// <summary>
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated at
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Created by user ID
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Updated by user ID
        /// </summary>
        public Guid? UpdatedBy { get; set; }
    }
}
