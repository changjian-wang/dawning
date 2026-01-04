using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Gateway
{
    /// <summary>
    /// IP access rule entity
    /// </summary>
    [Table("ip_access_rules")]
    public class IpAccessRuleEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// IP address or CIDR range (e.g., 192.168.1.0/24)
        /// </summary>
        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Rule type: whitelist, blacklist
        /// </summary>
        [Column("rule_type")]
        public string RuleType { get; set; } = "blacklist";

        /// <summary>
        /// Description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Expiration time (optional, for temporary bans)
        /// </summary>
        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Creator
        /// </summary>
        [Column("created_by")]
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Rate limit policy entity
    /// </summary>
    [Table("rate_limit_policies")]
    public class RateLimitPolicyEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Policy name (unique identifier)
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Policy type: fixed-window, sliding-window, token-bucket, concurrency
        /// </summary>
        [Column("policy_type")]
        public string PolicyType { get; set; } = "fixed-window";

        /// <summary>
        /// Allowed requests per window
        /// </summary>
        [Column("permit_limit")]
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Time window (seconds)
        /// </summary>
        [Column("window_seconds")]
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Sliding window segments count (only for sliding-window)
        /// </summary>
        [Column("segments_per_window")]
        public int SegmentsPerWindow { get; set; } = 6;

        /// <summary>
        /// Queue limit
        /// </summary>
        [Column("queue_limit")]
        public int QueueLimit { get; set; } = 0;

        /// <summary>
        /// Token replenishment rate (only for token-bucket)
        /// </summary>
        [Column("tokens_per_period")]
        public int TokensPerPeriod { get; set; } = 10;

        /// <summary>
        /// Token replenishment period (seconds, only for token-bucket)
        /// </summary>
        [Column("replenishment_period_seconds")]
        public int ReplenishmentPeriodSeconds { get; set; } = 1;

        /// <summary>
        /// Whether enabled
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
