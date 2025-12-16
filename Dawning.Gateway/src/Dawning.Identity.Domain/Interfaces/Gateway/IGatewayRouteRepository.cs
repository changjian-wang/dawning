using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// 网关路由仓储接口
    /// </summary>
    public interface IGatewayRouteRepository
    {
        /// <summary>
        /// 根据ID获取路由
        /// </summary>
        Task<GatewayRoute?> GetAsync(Guid id);

        /// <summary>
        /// 根据RouteId获取路由
        /// </summary>
        Task<GatewayRoute?> GetByRouteIdAsync(string routeId);

        /// <summary>
        /// 获取所有启用的路由
        /// </summary>
        Task<IEnumerable<GatewayRoute>> GetAllEnabledAsync();

        /// <summary>
        /// 获取分页路由列表
        /// </summary>
        Task<PagedData<GatewayRoute>> GetPagedListAsync(
            GatewayRouteQueryModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// 插入路由
        /// </summary>
        ValueTask<int> InsertAsync(GatewayRoute model);

        /// <summary>
        /// 更新路由
        /// </summary>
        ValueTask<int> UpdateAsync(GatewayRoute model);

        /// <summary>
        /// 删除路由
        /// </summary>
        ValueTask<int> DeleteAsync(Guid id);

        /// <summary>
        /// 检查RouteId是否已存在
        /// </summary>
        Task<bool> ExistsByRouteIdAsync(string routeId, Guid? excludeId = null);
    }
}
