namespace Dawning.Identity.Application.Dtos.Gateway
{
    /// <summary>
    /// IP 访问规则 DTO
    /// </summary>
    public class IpAccessRuleDto
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string RuleType { get; set; } = "blacklist";
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class CreateIpAccessRuleDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string RuleType { get; set; } = "blacklist";
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateIpAccessRuleDto
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string RuleType { get; set; } = "blacklist";
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// 限流策略 DTO
    /// </summary>
    public class RateLimitPolicyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string PolicyType { get; set; } = "fixed-window";
        public int PermitLimit { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
        public int SegmentsPerWindow { get; set; } = 6;
        public int QueueLimit { get; set; } = 0;
        public int TokensPerPeriod { get; set; } = 10;
        public int ReplenishmentPeriodSeconds { get; set; } = 1;
        public bool IsEnabled { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateRateLimitPolicyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string PolicyType { get; set; } = "fixed-window";
        public int PermitLimit { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
        public int SegmentsPerWindow { get; set; } = 6;
        public int QueueLimit { get; set; } = 0;
        public int TokensPerPeriod { get; set; } = 10;
        public int ReplenishmentPeriodSeconds { get; set; } = 1;
        public bool IsEnabled { get; set; } = true;
        public string? Description { get; set; }
    }

    public class UpdateRateLimitPolicyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string PolicyType { get; set; } = "fixed-window";
        public int PermitLimit { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
        public int SegmentsPerWindow { get; set; } = 6;
        public int QueueLimit { get; set; } = 0;
        public int TokensPerPeriod { get; set; } = 10;
        public int ReplenishmentPeriodSeconds { get; set; } = 1;
        public bool IsEnabled { get; set; } = true;
        public string? Description { get; set; }
    }
}
