using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Application.Interfaces.Gateway
{
    /// <summary>
    /// Gateway route service interface
    /// </summary>
    public interface IGatewayRouteService
    {
        /// <summary>
        /// Get route by ID
        /// </summary>
        Task<GatewayRouteDto?> GetAsync(Guid id);

        /// <summary>
        /// Get route by RouteId
        /// </summary>
        Task<GatewayRouteDto?> GetByRouteIdAsync(string routeId);

        /// <summary>
        /// Get all enabled routes (for YARP configuration)
        /// </summary>
        Task<IEnumerable<GatewayRouteDto>> GetAllEnabledAsync();

        /// <summary>
        /// Get paginated route list
        /// </summary>
        Task<PagedData<GatewayRouteDto>> GetPagedListAsync(
            GatewayRouteQueryModel queryModel,
            int page,
            int pageSize
        );

        /// <summary>
        /// Create route
        /// </summary>
        Task<GatewayRouteDto> CreateAsync(CreateGatewayRouteDto dto, string? username = null);

        /// <summary>
        /// Update route
        /// </summary>
        Task<GatewayRouteDto?> UpdateAsync(UpdateGatewayRouteDto dto, string? username = null);

        /// <summary>
        /// Delete route
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Toggle route enabled status
        /// </summary>
        Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null);
    }
}
