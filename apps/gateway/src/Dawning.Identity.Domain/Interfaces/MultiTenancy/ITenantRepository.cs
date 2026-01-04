using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Domain.Interfaces.MultiTenancy
{
    /// <summary>
    /// Tenant repository interface
    /// </summary>
    public interface ITenantRepository
    {
        /// <summary>
        /// Get tenant by ID
        /// </summary>
        Task<Tenant?> GetAsync(Guid id);

        /// <summary>
        /// Get tenant by code
        /// </summary>
        Task<Tenant?> GetByCodeAsync(string code);

        /// <summary>
        /// Get tenant by domain
        /// </summary>
        Task<Tenant?> GetByDomainAsync(string domain);

        /// <summary>
        /// Get all tenants
        /// </summary>
        Task<IEnumerable<Tenant>> GetAllAsync();

        /// <summary>
        /// Get all active tenants
        /// </summary>
        Task<IEnumerable<Tenant>> GetActiveTenantsAsync();

        /// <summary>
        /// Get paginated tenant list
        /// </summary>
        Task<PagedData<Tenant>> GetPagedAsync(
            string? keyword,
            bool? isActive,
            int page,
            int pageSize
        );

        /// <summary>
        /// Create tenant
        /// </summary>
        Task<int> InsertAsync(Tenant tenant);

        /// <summary>
        /// Update tenant
        /// </summary>
        Task<int> UpdateAsync(Tenant tenant);

        /// <summary>
        /// Delete tenant
        /// </summary>
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Check if tenant code exists
        /// </summary>
        Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null);

        /// <summary>
        /// Check if domain exists
        /// </summary>
        Task<bool> ExistsDomainAsync(string domain, Guid? excludeId = null);
    }
}
