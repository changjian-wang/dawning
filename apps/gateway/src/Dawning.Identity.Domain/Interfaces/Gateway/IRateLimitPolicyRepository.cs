using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// Rate limit policy repository interface
    /// </summary>
    public interface IRateLimitPolicyRepository
    {
        /// <summary>
        /// Get policy by ID
        /// </summary>
        Task<RateLimitPolicy?> GetAsync(Guid id);

        /// <summary>
        /// Get policy by name
        /// </summary>
        Task<RateLimitPolicy?> GetByNameAsync(string name);

        /// <summary>
        /// Get all policies
        /// </summary>
        Task<IEnumerable<RateLimitPolicy>> GetAllAsync();

        /// <summary>
        /// Get all enabled policies
        /// </summary>
        Task<IEnumerable<RateLimitPolicy>> GetAllEnabledAsync();

        /// <summary>
        /// Insert policy
        /// </summary>
        ValueTask<int> InsertAsync(RateLimitPolicy model);

        /// <summary>
        /// Update policy
        /// </summary>
        ValueTask<bool> UpdateAsync(RateLimitPolicy model);

        /// <summary>
        /// Delete policy
        /// </summary>
        ValueTask<bool> DeleteAsync(Guid id);
    }
}
