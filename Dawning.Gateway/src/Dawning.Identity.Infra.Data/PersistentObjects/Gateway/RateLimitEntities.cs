using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Gateway
{
    /// <summary>
    /// IP 访问规则实体
    /// </summary>
    [Table("ip_access_rules")]
    public class IpAccessRuleEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// IP 地址或 CIDR 范围 (如 192.168.1.0/24)
        /// </summary>
        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 规则类型: whitelist, blacklist
        /// </summary>
        [Column("rule_type")]
        public string RuleType { get; set; } = "blacklist";

        /// <summary>
        /// 描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 过期时间（可选，用于临时封禁）
        /// </summary>
        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        [Column("created_by")]
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// 限流策略实体
    /// </summary>
    [Table("rate_limit_policies")]
    public class RateLimitPolicyEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 策略名称（唯一标识）
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 策略类型: fixed-window, sliding-window, token-bucket, concurrency
        /// </summary>
        [Column("policy_type")]
        public string PolicyType { get; set; } = "fixed-window";

        /// <summary>
        /// 每窗口允许的请求数
        /// </summary>
        [Column("permit_limit")]
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// 时间窗口（秒）
        /// </summary>
        [Column("window_seconds")]
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// 滑动窗口分段数（仅用于 sliding-window）
        /// </summary>
        [Column("segments_per_window")]
        public int SegmentsPerWindow { get; set; } = 6;

        /// <summary>
        /// 队列限制
        /// </summary>
        [Column("queue_limit")]
        public int QueueLimit { get; set; } = 0;

        /// <summary>
        /// 令牌补充速率（仅用于 token-bucket）
        /// </summary>
        [Column("tokens_per_period")]
        public int TokensPerPeriod { get; set; } = 10;

        /// <summary>
        /// 令牌补充周期（秒，仅用于 token-bucket）
        /// </summary>
        [Column("replenishment_period_seconds")]
        public int ReplenishmentPeriodSeconds { get; set; } = 1;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
