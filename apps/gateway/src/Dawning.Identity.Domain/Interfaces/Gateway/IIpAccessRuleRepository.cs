using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// IP access rule repository interface
    /// </summary>
    public interface IIpAccessRuleRepository
    {
        /// <summary>
        /// Get rule by ID
        /// </summary>
        Task<IpAccessRule?> GetAsync(Guid id);

        /// <summary>
        /// Get paginated rule list
        /// </summary>
        Task<PagedData<IpAccessRule>> GetPagedListAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        );

        /// <summary>
        /// Get active rules by type
        /// </summary>
        Task<IEnumerable<IpAccessRule>> GetActiveRulesByTypeAsync(string ruleType);

        /// <summary>
        /// Check if IP is blacklisted
        /// </summary>
        Task<bool> IsIpBlacklistedAsync(string ipAddress);

        /// <summary>
        /// Check if IP is whitelisted
        /// </summary>
        Task<bool> IsIpWhitelistedAsync(string ipAddress);

        /// <summary>
        /// Insert rule
        /// </summary>
        ValueTask<int> InsertAsync(IpAccessRule model);

        /// <summary>
        /// Update rule
        /// </summary>
        ValueTask<bool> UpdateAsync(IpAccessRule model);

        /// <summary>
        /// Delete rule
        /// </summary>
        ValueTask<bool> DeleteAsync(Guid id);
    }
}
