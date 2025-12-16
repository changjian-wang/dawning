using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Interfaces.UoW;

namespace Dawning.Identity.Api.Services
{
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

    public class RateLimitService : IRateLimitService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RateLimitService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Rate Limit Policies

        public async Task<IEnumerable<RateLimitPolicyDto>> GetAllPoliciesAsync()
        {
            var policies = await _unitOfWork.RateLimitPolicy.GetAllAsync();
            return policies.Select(MapToDto);
        }

        public async Task<RateLimitPolicyDto?> GetPolicyByIdAsync(Guid id)
        {
            var policy = await _unitOfWork.RateLimitPolicy.GetAsync(id);
            return policy != null ? MapToDto(policy) : null;
        }

        public async Task<RateLimitPolicyDto?> GetPolicyByNameAsync(string name)
        {
            var policy = await _unitOfWork.RateLimitPolicy.GetByNameAsync(name);
            return policy != null ? MapToDto(policy) : null;
        }

        public async Task<Guid> CreatePolicyAsync(CreateRateLimitPolicyDto dto)
        {
            var id = Guid.NewGuid();
            var policy = new RateLimitPolicy
            {
                Id = id,
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                PolicyType = dto.PolicyType,
                PermitLimit = dto.PermitLimit,
                WindowSeconds = dto.WindowSeconds,
                SegmentsPerWindow = dto.SegmentsPerWindow,
                QueueLimit = dto.QueueLimit,
                TokensPerPeriod = dto.TokensPerPeriod,
                ReplenishmentPeriodSeconds = dto.ReplenishmentPeriodSeconds,
                IsEnabled = dto.IsEnabled,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.RateLimitPolicy.InsertAsync(policy);
            await _unitOfWork.CommitAsync();
            return id;
        }

        public async Task<bool> UpdatePolicyAsync(UpdateRateLimitPolicyDto dto)
        {
            var existingPolicy = await _unitOfWork.RateLimitPolicy.GetAsync(dto.Id);
            if (existingPolicy == null)
                return false;

            existingPolicy.Name = dto.Name;
            existingPolicy.DisplayName = dto.DisplayName;
            existingPolicy.PolicyType = dto.PolicyType;
            existingPolicy.PermitLimit = dto.PermitLimit;
            existingPolicy.WindowSeconds = dto.WindowSeconds;
            existingPolicy.SegmentsPerWindow = dto.SegmentsPerWindow;
            existingPolicy.QueueLimit = dto.QueueLimit;
            existingPolicy.TokensPerPeriod = dto.TokensPerPeriod;
            existingPolicy.ReplenishmentPeriodSeconds = dto.ReplenishmentPeriodSeconds;
            existingPolicy.IsEnabled = dto.IsEnabled;
            existingPolicy.Description = dto.Description;
            existingPolicy.UpdatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.RateLimitPolicy.UpdateAsync(existingPolicy);
            await _unitOfWork.CommitAsync();
            return result;
        }

        public async Task<bool> DeletePolicyAsync(Guid id)
        {
            var result = await _unitOfWork.RateLimitPolicy.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
            return result;
        }

        #endregion

        #region IP Access Rules

        public async Task<(IEnumerable<IpAccessRuleDto> Items, long Total)> GetIpRulesPagedAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        )
        {
            var result = await _unitOfWork.IpAccessRule.GetPagedListAsync(
                ruleType,
                isEnabled,
                page,
                pageSize
            );
            return (result.Items.Select(MapToDto), result.TotalCount);
        }

        public async Task<IpAccessRuleDto?> GetIpRuleByIdAsync(Guid id)
        {
            var rule = await _unitOfWork.IpAccessRule.GetAsync(id);
            return rule != null ? MapToDto(rule) : null;
        }

        public async Task<IEnumerable<IpAccessRuleDto>> GetActiveRulesByTypeAsync(string ruleType)
        {
            var rules = await _unitOfWork.IpAccessRule.GetActiveRulesByTypeAsync(ruleType);
            return rules.Select(MapToDto);
        }

        public async Task<Guid> CreateIpRuleAsync(CreateIpAccessRuleDto dto, string? createdBy)
        {
            var id = Guid.NewGuid();
            var rule = new IpAccessRule
            {
                Id = id,
                IpAddress = dto.IpAddress,
                RuleType = dto.RuleType,
                Description = dto.Description,
                IsEnabled = dto.IsEnabled,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
            };

            await _unitOfWork.IpAccessRule.InsertAsync(rule);
            await _unitOfWork.CommitAsync();
            return id;
        }

        public async Task<bool> UpdateIpRuleAsync(UpdateIpAccessRuleDto dto)
        {
            var existingRule = await _unitOfWork.IpAccessRule.GetAsync(dto.Id);
            if (existingRule == null)
                return false;

            existingRule.IpAddress = dto.IpAddress;
            existingRule.RuleType = dto.RuleType;
            existingRule.Description = dto.Description;
            existingRule.IsEnabled = dto.IsEnabled;
            existingRule.ExpiresAt = dto.ExpiresAt;
            existingRule.UpdatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.IpAccessRule.UpdateAsync(existingRule);
            await _unitOfWork.CommitAsync();
            return result;
        }

        public async Task<bool> DeleteIpRuleAsync(Guid id)
        {
            var result = await _unitOfWork.IpAccessRule.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
            return result;
        }

        public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            return await _unitOfWork.IpAccessRule.IsIpBlacklistedAsync(ipAddress);
        }

        public async Task<bool> IsIpWhitelistedAsync(string ipAddress)
        {
            return await _unitOfWork.IpAccessRule.IsIpWhitelistedAsync(ipAddress);
        }

        #endregion

        #region Mappers

        private static RateLimitPolicyDto MapToDto(RateLimitPolicy model)
        {
            return new RateLimitPolicyDto
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

        private static IpAccessRuleDto MapToDto(IpAccessRule model)
        {
            return new IpAccessRuleDto
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

        #endregion
    }
}
