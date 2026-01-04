using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// Gateway cluster repository interface
    /// </summary>
    public interface IGatewayClusterRepository
    {
        /// <summary>
        /// Get cluster by ID
        /// </summary>
        Task<GatewayCluster?> GetAsync(Guid id);

        /// <summary>
        /// Get cluster by ClusterId
        /// </summary>
        Task<GatewayCluster?> GetByClusterIdAsync(string clusterId);

        /// <summary>
        /// Get all enabled clusters
        /// </summary>
        Task<IEnumerable<GatewayCluster>> GetAllEnabledAsync();

        /// <summary>
        /// Get all clusters (for dropdown selection)
        /// </summary>
        Task<IEnumerable<GatewayCluster>> GetAllAsync();

        /// <summary>
        /// Get paginated cluster list
        /// </summary>
        Task<PagedData<GatewayCluster>> GetPagedListAsync(
            GatewayClusterQueryModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// Insert cluster
        /// </summary>
        ValueTask<int> InsertAsync(GatewayCluster model);

        /// <summary>
        /// Update cluster
        /// </summary>
        ValueTask<int> UpdateAsync(GatewayCluster model);

        /// <summary>
        /// Delete cluster
        /// </summary>
        ValueTask<int> DeleteAsync(Guid id);

        /// <summary>
        /// Check if ClusterId already exists
        /// </summary>
        Task<bool> ExistsByClusterIdAsync(string clusterId, Guid? excludeId = null);

        /// <summary>
        /// Check if cluster is referenced by routes
        /// </summary>
        Task<bool> IsReferencedByRoutesAsync(string clusterId);
    }
}
