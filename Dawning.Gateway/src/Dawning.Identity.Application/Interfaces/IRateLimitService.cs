using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;

namespace Dawning.Identity.Application.Interfaces
{
    /// <summary>
    /// 限流和访问控制服务接口
    /// </summary>
    public interface IRateLimitService
    {
        // Rate Limit Policies
        Task<IEnumerable<RateLimitPolicyDto>> GetAllPoliciesAsync();
        Task<RateLimitPolicyDto?> GetPolicyByIdAsync(Guid id);
        Task<RateLimitPolicyDto?> GetPolicyByNameAsync(string name);
        Task<Guid> CreatePolicyAsync(CreateRateLimitPolicyDto dto);
        Task<bool> UpdatePolicyAsync(UpdateRateLimitPolicyDto dto);
        Task<bool> DeletePolicyAsync(Guid id);

        // IP Access Rules
        Task<(IEnumerable<IpAccessRuleDto> Items, long Total)> GetIpRulesPagedAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        );
        Task<IpAccessRuleDto?> GetIpRuleByIdAsync(Guid id);
        Task<IEnumerable<IpAccessRuleDto>> GetActiveRulesByTypeAsync(string ruleType);
        Task<Guid> CreateIpRuleAsync(CreateIpAccessRuleDto dto, string? createdBy);
        Task<bool> UpdateIpRuleAsync(UpdateIpAccessRuleDto dto);
        Task<bool> DeleteIpRuleAsync(Guid id);
        Task<bool> IsIpBlacklistedAsync(string ipAddress);
        Task<bool> IsIpWhitelistedAsync(string ipAddress);
    }
}
