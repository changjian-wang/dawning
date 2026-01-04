using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Application.Interfaces.Gateway;
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
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GatewayRouteService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// Get route by ID
        /// </summary>
        public async Task<GatewayRouteDto?> GetAsync(Guid id)
        {
            var route = await _uow.GatewayRoute.GetAsync(id);
            return route == null ? null : _mapper.Map<GatewayRouteDto>(route);
        }

        /// <summary>
        /// Get route by RouteId
        /// </summary>
        public async Task<GatewayRouteDto?> GetByRouteIdAsync(string routeId)
        {
            var route = await _uow.GatewayRoute.GetByRouteIdAsync(routeId);
            return route == null ? null : _mapper.Map<GatewayRouteDto>(route);
        }

        /// <summary>
        /// Get all enabled routes
        /// </summary>
        public async Task<IEnumerable<GatewayRouteDto>> GetAllEnabledAsync()
        {
            var routes = await _uow.GatewayRoute.GetAllEnabledAsync();
            return _mapper.Map<IEnumerable<GatewayRouteDto>>(routes);
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
            var pagedData = await _uow.GatewayRoute.GetPagedListAsync(queryModel, page, pageSize);
            return new PagedData<GatewayRouteDto>
            {
                Items = _mapper.Map<IEnumerable<GatewayRouteDto>>(pagedData.Items),
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
            if (await _uow.GatewayRoute.ExistsByRouteIdAsync(dto.RouteId))
            {
                throw new InvalidOperationException($"Route ID '{dto.RouteId}' already exists");
            }

            // Check if cluster exists
            var cluster = await _uow.GatewayCluster.GetByClusterIdAsync(dto.ClusterId);
            if (cluster == null)
            {
                throw new InvalidOperationException($"Cluster '{dto.ClusterId}' not found");
            }

            var route = _mapper.Map<GatewayRoute>(dto);
            route.Id = Guid.NewGuid();
            route.CreatedAt = DateTime.UtcNow;
            route.CreatedBy = username;

            await _uow.GatewayRoute.InsertAsync(route);

            return _mapper.Map<GatewayRouteDto>(route);
        }

        /// <summary>
        /// Update route
        /// </summary>
        public async Task<GatewayRouteDto?> UpdateAsync(
            UpdateGatewayRouteDto dto,
            string? username = null
        )
        {
            var existing = await _uow.GatewayRoute.GetAsync(dto.Id);
            if (existing == null)
            {
                return null;
            }

            // Check if RouteId is used by other records
            if (await _uow.GatewayRoute.ExistsByRouteIdAsync(dto.RouteId, dto.Id))
            {
                throw new InvalidOperationException($"Route ID '{dto.RouteId}' is already used by another route");
            }

            // Check if cluster exists
            var cluster = await _uow.GatewayCluster.GetByClusterIdAsync(dto.ClusterId);
            if (cluster == null)
            {
                throw new InvalidOperationException($"Cluster '{dto.ClusterId}' not found");
            }

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = username;

            await _uow.GatewayRoute.UpdateAsync(existing);

            return _mapper.Map<GatewayRouteDto>(existing);
        }

        /// <summary>
        /// Delete route
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _uow.GatewayRoute.DeleteAsync(id);
            return result > 0;
        }

        /// <summary>
        /// Toggle route enabled status
        /// </summary>
        public async Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null)
        {
            var route = await _uow.GatewayRoute.GetAsync(id);
            if (route == null)
            {
                return false;
            }

            route.IsEnabled = isEnabled;
            route.UpdatedAt = DateTime.UtcNow;
            route.UpdatedBy = username;

            var result = await _uow.GatewayRoute.UpdateAsync(route);
            return result > 0;
        }
    }
}
