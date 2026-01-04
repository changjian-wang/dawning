using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Application.Interfaces.MultiTenancy
{
    /// <summary>
    /// Tenant service interface
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Get tenant by ID
        /// </summary>
        Task<Tenant?> GetAsync(Guid id);

        /// <summary>
        /// Get tenant by tenant code
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
        /// Get paged tenant list
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
        Task<Tenant> CreateAsync(Tenant tenant);

        /// <summary>
        /// Update tenant
        /// </summary>
        Task<Tenant> UpdateAsync(Tenant tenant);

        /// <summary>
        /// Delete tenant
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Toggle tenant active status
        /// </summary>
        Task<bool> SetActiveAsync(Guid id, bool isActive);

        /// <summary>
        /// Check if tenant code is available
        /// </summary>
        Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null);

        /// <summary>
        /// Check if domain is available
        /// </summary>
        Task<bool> IsDomainAvailableAsync(string domain, Guid? excludeId = null);

        /// <summary>
        /// Resolve tenant (from request header, domain, query params, etc.)
        /// </summary>
        Task<Tenant?> ResolveTenantAsync(string? tenantCode, string? host);
    }
}
