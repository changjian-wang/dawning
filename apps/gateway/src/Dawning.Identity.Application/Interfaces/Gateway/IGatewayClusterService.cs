using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Application.Interfaces.Gateway
{
    /// <summary>
    /// Gateway cluster service interface
    /// </summary>
    public interface IGatewayClusterService
    {
        /// <summary>
        /// Get cluster by ID
        /// </summary>
        Task<GatewayClusterDto?> GetAsync(Guid id);

        /// <summary>
        /// Get cluster by ClusterId
        /// </summary>
        Task<GatewayClusterDto?> GetByClusterIdAsync(string clusterId);

        /// <summary>
        /// Get all enabled clusters (for YARP configuration)
        /// </summary>
        Task<IEnumerable<GatewayClusterDto>> GetAllEnabledAsync();

        /// <summary>
        /// Get cluster options list (for dropdown selection)
        /// </summary>
        Task<IEnumerable<ClusterOptionDto>> GetOptionsAsync();

        /// <summary>
        /// Get paginated cluster list
        /// </summary>
        Task<PagedData<GatewayClusterDto>> GetPagedListAsync(
            GatewayClusterQueryModel queryModel,
            int page,
            int pageSize
        );

        /// <summary>
        /// Create cluster
        /// </summary>
        Task<GatewayClusterDto> CreateAsync(CreateGatewayClusterDto dto, string? username = null);

        /// <summary>
        /// Update cluster
        /// </summary>
        Task<GatewayClusterDto?> UpdateAsync(UpdateGatewayClusterDto dto, string? username = null);

        /// <summary>
        /// Delete cluster
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id);

        /// <summary>
        /// Toggle cluster enabled status
        /// </summary>
        Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null);
    }
}
