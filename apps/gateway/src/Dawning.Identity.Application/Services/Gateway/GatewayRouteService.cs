using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Application.Interfaces.Gateway;
using Dawning.Identity.Application.Mapping.Gateway;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Application.Services.Gateway
{
    /// <summary>
    /// Gateway route service implementation
    /// </summary>
    public class GatewayRouteService : IGatewayRouteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GatewayRouteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get route by ID
        /// </summary>
        public async Task<GatewayRouteDto?> GetAsync(Guid id)
        {
            var route = await _unitOfWork.GatewayRoute.GetAsync(id);
            return route.ToDtoOrNull();
        }

        /// <summary>
        /// Get route by RouteId
        /// </summary>
        public async Task<GatewayRouteDto?> GetByRouteIdAsync(string routeId)
        {
            var route = await _unitOfWork.GatewayRoute.GetByRouteIdAsync(routeId);
            return route.ToDtoOrNull();
        }

        /// <summary>
        /// Get all enabled routes
        /// </summary>
        public async Task<IEnumerable<GatewayRouteDto>> GetAllEnabledAsync()
        {
            var routes = await _unitOfWork.GatewayRoute.GetAllEnabledAsync();
            return routes.ToDtos();
        }

        /// <summary>
        /// Get paged route list
        /// </summary>
        public async Task<PagedData<GatewayRouteDto>> GetPagedListAsync(
            GatewayRouteQueryModel queryModel,
            int page,
            int pageSize
        )
        {
            var pagedData = await _unitOfWork.GatewayRoute.GetPagedListAsync(
                queryModel,
                page,
                pageSize
            );
            return new PagedData<GatewayRouteDto>
            {
                Items = pagedData.Items.ToDtos(),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
            };
        }

        /// <summary>
        /// Create route
        /// </summary>
        public async Task<GatewayRouteDto> CreateAsync(
            CreateGatewayRouteDto dto,
            string? username = null
        )
        {
            // Check if RouteId already exists
            if (await _unitOfWork.GatewayRoute.ExistsByRouteIdAsync(dto.RouteId))
            {
                throw new InvalidOperationException($"Route ID '{dto.RouteId}' already exists");
            }

            // Check if cluster exists
            var cluster = await _unitOfWork.GatewayCluster.GetByClusterIdAsync(dto.ClusterId);
            if (cluster == null)
            {
                throw new InvalidOperationException($"Cluster '{dto.ClusterId}' not found");
            }

            var route = dto.ToEntity();
            route.CreatedBy = username;

            await _unitOfWork.GatewayRoute.InsertAsync(route);

            return route.ToDto();
        }

        /// <summary>
        /// Update route
        /// </summary>
        public async Task<GatewayRouteDto?> UpdateAsync(
            UpdateGatewayRouteDto dto,
            string? username = null
        )
        {
            var existing = await _unitOfWork.GatewayRoute.GetAsync(dto.Id);
            if (existing == null)
            {
                return null;
            }

            // Check if RouteId is used by other records
            if (await _unitOfWork.GatewayRoute.ExistsByRouteIdAsync(dto.RouteId, dto.Id))
            {
                throw new InvalidOperationException(
                    $"Route ID '{dto.RouteId}' is already used by another route"
                );
            }

            // Check if cluster exists
            var cluster = await _unitOfWork.GatewayCluster.GetByClusterIdAsync(dto.ClusterId);
            if (cluster == null)
            {
                throw new InvalidOperationException($"Cluster '{dto.ClusterId}' not found");
            }

            existing.ApplyUpdate(dto);
            existing.UpdatedBy = username;

            await _unitOfWork.GatewayRoute.UpdateAsync(existing);

            return existing.ToDto();
        }

        /// <summary>
        /// Delete route
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _unitOfWork.GatewayRoute.DeleteAsync(id);
            return result > 0;
        }

        /// <summary>
        /// Toggle route enabled status
        /// </summary>
        public async Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null)
        {
            var route = await _unitOfWork.GatewayRoute.GetAsync(id);
            if (route == null)
            {
                return false;
            }

            route.IsEnabled = isEnabled;
            route.UpdatedAt = DateTime.UtcNow;
            route.UpdatedBy = username;

            var result = await _unitOfWork.GatewayRoute.UpdateAsync(route);
            return result > 0;
        }
    }
}
