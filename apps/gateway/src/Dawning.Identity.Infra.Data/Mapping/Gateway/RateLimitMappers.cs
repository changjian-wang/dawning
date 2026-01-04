using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// RateLimitPolicy entity and domain model mapper
    /// </summary>
    public static class RateLimitPolicyMappers
    {
        private static IMapper Mapper { get; }

        static RateLimitPolicyMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RateLimitProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static RateLimitPolicy ToModel(this RateLimitPolicyEntity entity)
        {
            return Mapper.Map<RateLimitPolicy>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static RateLimitPolicyEntity ToEntity(this RateLimitPolicy model)
        {
            return Mapper.Map<RateLimitPolicyEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<RateLimitPolicy> ToModels(this IEnumerable<RateLimitPolicyEntity> entities)
        {
            return Mapper.Map<IEnumerable<RateLimitPolicy>>(entities);
        }
    }

    /// <summary>
    /// IpAccessRule entity and domain model mapper
    /// </summary>
    public static class IpAccessRuleMappers
    {
        private static IMapper Mapper { get; }

        static IpAccessRuleMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RateLimitProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model
        /// </summary>
        public static IpAccessRule ToModel(this IpAccessRuleEntity entity)
        {
            return Mapper.Map<IpAccessRule>(entity);
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static IpAccessRuleEntity ToEntity(this IpAccessRule model)
        {
            return Mapper.Map<IpAccessRuleEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<IpAccessRule> ToModels(this IEnumerable<IpAccessRuleEntity> entities)
        {
            return Mapper.Map<IEnumerable<IpAccessRule>>(entities);
        }
    }
}
