using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;

namespace Dawning.Identity.Infra.Data.Mapping.Gateway
{
    /// <summary>
    /// RateLimitPolicy and IpAccessRule entity and domain model mapping configuration
    /// </summary>
    public class RateLimitProfile : Profile
    {
        public RateLimitProfile()
        {
            // RateLimitPolicy: Entity -> Domain Model
            CreateMap<RateLimitPolicyEntity, RateLimitPolicy>();

            // RateLimitPolicy: Domain Model -> Entity
            CreateMap<RateLimitPolicy, RateLimitPolicyEntity>();

            // IpAccessRule: Entity -> Domain Model
            CreateMap<IpAccessRuleEntity, IpAccessRule>();

            // IpAccessRule: Domain Model -> Entity
            CreateMap<IpAccessRule, IpAccessRuleEntity>();
        }
    }
}
