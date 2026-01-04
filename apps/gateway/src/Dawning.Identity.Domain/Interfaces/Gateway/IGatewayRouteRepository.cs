using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// Gateway route repository interface
    /// </summary>
    public interface IGatewayRouteRepository
    {
        /// <summary>
        /// Get route by ID
        /// </summary>
        Task<GatewayRoute?> GetAsync(Guid id);

        /// <summary>
        /// Get route by RouteId
        /// </summary>
        Task<GatewayRoute?> GetByRouteIdAsync(string routeId);

        /// <summary>
        /// Get all enabled routes
        /// </summary>
        Task<IEnumerable<GatewayRoute>> GetAllEnabledAsync();

        /// <summary>
        /// Get paginated route list
        /// </summary>
        Task<PagedData<GatewayRoute>> GetPagedListAsync(
            GatewayRouteQueryModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// Insert route
        /// </summary>
        ValueTask<int> InsertAsync(GatewayRoute model);

        /// <summary>
        /// Update route
        /// </summary>
        ValueTask<int> UpdateAsync(GatewayRoute model);

        /// <summary>
        /// Delete route
        /// </summary>
        ValueTask<int> DeleteAsync(Guid id);

        /// <summary>
        /// Check if RouteId already exists
        /// </summary>
        Task<bool> ExistsByRouteIdAsync(string routeId, Guid? excludeId = null);
    }
}
