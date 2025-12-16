using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// 限流策略映射器
    /// </summary>
    public static class RateLimitPolicyMappers
    {
        /// <summary>
        /// 将数据库实体转换为领域模型
        /// </summary>
        public static RateLimitPolicy ToModel(this RateLimitPolicyEntity entity)
        {
            return new RateLimitPolicy
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                PolicyType = entity.PolicyType,
                PermitLimit = entity.PermitLimit,
                WindowSeconds = entity.WindowSeconds,
                SegmentsPerWindow = entity.SegmentsPerWindow,
                QueueLimit = entity.QueueLimit,
                TokensPerPeriod = entity.TokensPerPeriod,
                ReplenishmentPeriodSeconds = entity.ReplenishmentPeriodSeconds,
                IsEnabled = entity.IsEnabled,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        /// <summary>
        /// 将领域模型转换为数据库实体
        /// </summary>
        public static RateLimitPolicyEntity ToEntity(this RateLimitPolicy model)
        {
            return new RateLimitPolicyEntity
            {
                Id = model.Id,
                Name = model.Name,
                DisplayName = model.DisplayName,
                PolicyType = model.PolicyType,
                PermitLimit = model.PermitLimit,
                WindowSeconds = model.WindowSeconds,
                SegmentsPerWindow = model.SegmentsPerWindow,
                QueueLimit = model.QueueLimit,
                TokensPerPeriod = model.TokensPerPeriod,
                ReplenishmentPeriodSeconds = model.ReplenishmentPeriodSeconds,
                IsEnabled = model.IsEnabled,
                Description = model.Description,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
            };
        }

        /// <summary>
        /// 批量转换实体到模型
        /// </summary>
        public static IEnumerable<RateLimitPolicy> ToModels(
            this IEnumerable<RateLimitPolicyEntity> entities
        )
        {
            return entities.Select(e => e.ToModel());
        }
    }

    /// <summary>
    /// IP 访问规则映射器
    /// </summary>
    public static class IpAccessRuleMappers
    {
        /// <summary>
        /// 将数据库实体转换为领域模型
        /// </summary>
        public static IpAccessRule ToModel(this IpAccessRuleEntity entity)
        {
            return new IpAccessRule
            {
                Id = entity.Id,
                IpAddress = entity.IpAddress,
                RuleType = entity.RuleType,
                Description = entity.Description,
                IsEnabled = entity.IsEnabled,
                ExpiresAt = entity.ExpiresAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy,
            };
        }

        /// <summary>
        /// 将领域模型转换为数据库实体
        /// </summary>
        public static IpAccessRuleEntity ToEntity(this IpAccessRule model)
        {
            return new IpAccessRuleEntity
            {
                Id = model.Id,
                IpAddress = model.IpAddress,
                RuleType = model.RuleType,
                Description = model.Description,
                IsEnabled = model.IsEnabled,
                ExpiresAt = model.ExpiresAt,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy,
            };
        }

        /// <summary>
        /// 批量转换实体到模型
        /// </summary>
        public static IEnumerable<IpAccessRule> ToModels(
            this IEnumerable<IpAccessRuleEntity> entities
        )
        {
            return entities.Select(e => e.ToModel());
        }
    }
}
