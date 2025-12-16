using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Application.Interfaces.Gateway
{
    /// <summary>
    /// 网关路由服务接口
    /// </summary>
    public interface IGatewayRouteService
    {
        /// <summary>
        /// 根据ID获取路由
        /// </summary>
        Task<GatewayRouteDto?> GetAsync(Guid id);

        /// <summary>
        /// 根据RouteId获取路由
        /// </summary>
        Task<GatewayRouteDto?> GetByRouteIdAsync(string routeId);

        /// <summary>
        /// 获取所有启用的路由（用于YARP配置）
        /// </summary>
        Task<IEnumerable<GatewayRouteDto>> GetAllEnabledAsync();

        /// <summary>
        /// 获取分页路由列表
        /// </summary>
        Task<PagedData<GatewayRouteDto>> GetPagedListAsync(
            GatewayRouteQueryModel queryModel,
            int page,
            int pageSize
        );

        /// <summary>
        /// 创建路由
        /// </summary>
        Task<GatewayRouteDto> CreateAsync(CreateGatewayRouteDto dto, string? username = null);

        /// <summary>
        /// 更新路由
        /// </summary>
        Task<GatewayRouteDto?> UpdateAsync(UpdateGatewayRouteDto dto, string? username = null);

        /// <summary>
        /// 删除路由
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 切换路由启用状态
        /// </summary>
        Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null);
    }
}
