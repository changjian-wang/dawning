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
    /// 网关路由服务实现
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
        /// 根据ID获取路由
        /// </summary>
        public async Task<GatewayRouteDto?> GetAsync(Guid id)
        {
            var route = await _uow.GatewayRoute.GetAsync(id);
            return route == null ? null : _mapper.Map<GatewayRouteDto>(route);
        }

        /// <summary>
        /// 根据RouteId获取路由
        /// </summary>
        public async Task<GatewayRouteDto?> GetByRouteIdAsync(string routeId)
        {
            var route = await _uow.GatewayRoute.GetByRouteIdAsync(routeId);
            return route == null ? null : _mapper.Map<GatewayRouteDto>(route);
        }

        /// <summary>
        /// 获取所有启用的路由
        /// </summary>
        public async Task<IEnumerable<GatewayRouteDto>> GetAllEnabledAsync()
        {
            var routes = await _uow.GatewayRoute.GetAllEnabledAsync();
            return _mapper.Map<IEnumerable<GatewayRouteDto>>(routes);
        }

        /// <summary>
        /// 获取分页路由列表
        /// </summary>
        public async Task<PagedData<GatewayRouteDto>> GetPagedListAsync(
            GatewayRouteQueryModel queryModel,
            int page,
            int pageSize)
        {
            var pagedData = await _uow.GatewayRoute.GetPagedListAsync(queryModel, page, pageSize);
            return new PagedData<GatewayRouteDto>
            {
                Items = _mapper.Map<IEnumerable<GatewayRouteDto>>(pagedData.Items),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize
            };
        }

        /// <summary>
        /// 创建路由
        /// </summary>
        public async Task<GatewayRouteDto> CreateAsync(CreateGatewayRouteDto dto, string? username = null)
        {
            // 检查RouteId是否已存在
            if (await _uow.GatewayRoute.ExistsByRouteIdAsync(dto.RouteId))
            {
                throw new InvalidOperationException($"路由ID '{dto.RouteId}' 已存在");
            }

            // 检查集群是否存在
            var cluster = await _uow.GatewayCluster.GetByClusterIdAsync(dto.ClusterId);
            if (cluster == null)
            {
                throw new InvalidOperationException($"集群 '{dto.ClusterId}' 不存在");
            }

            var route = _mapper.Map<GatewayRoute>(dto);
            route.Id = Guid.NewGuid();
            route.CreatedAt = DateTime.UtcNow;
            route.CreatedBy = username;

            await _uow.GatewayRoute.InsertAsync(route);

            return _mapper.Map<GatewayRouteDto>(route);
        }

        /// <summary>
        /// 更新路由
        /// </summary>
        public async Task<GatewayRouteDto?> UpdateAsync(UpdateGatewayRouteDto dto, string? username = null)
        {
            var existing = await _uow.GatewayRoute.GetAsync(dto.Id);
            if (existing == null)
            {
                return null;
            }

            // 检查RouteId是否被其他记录使用
            if (await _uow.GatewayRoute.ExistsByRouteIdAsync(dto.RouteId, dto.Id))
            {
                throw new InvalidOperationException($"路由ID '{dto.RouteId}' 已被其他路由使用");
            }

            // 检查集群是否存在
            var cluster = await _uow.GatewayCluster.GetByClusterIdAsync(dto.ClusterId);
            if (cluster == null)
            {
                throw new InvalidOperationException($"集群 '{dto.ClusterId}' 不存在");
            }

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = username;

            await _uow.GatewayRoute.UpdateAsync(existing);

            return _mapper.Map<GatewayRouteDto>(existing);
        }

        /// <summary>
        /// 删除路由
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _uow.GatewayRoute.DeleteAsync(id);
            return result > 0;
        }

        /// <summary>
        /// 切换路由启用状态
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
