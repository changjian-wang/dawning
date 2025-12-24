using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// 网关集群仓储接口
    /// </summary>
    public interface IGatewayClusterRepository
    {
        /// <summary>
        /// 根据ID获取集群
        /// </summary>
        Task<GatewayCluster?> GetAsync(Guid id);

        /// <summary>
        /// 根据ClusterId获取集群
        /// </summary>
        Task<GatewayCluster?> GetByClusterIdAsync(string clusterId);

        /// <summary>
        /// 获取所有启用的集群
        /// </summary>
        Task<IEnumerable<GatewayCluster>> GetAllEnabledAsync();

        /// <summary>
        /// 获取所有集群（用于下拉选择）
        /// </summary>
        Task<IEnumerable<GatewayCluster>> GetAllAsync();

        /// <summary>
        /// 获取分页集群列表
        /// </summary>
        Task<PagedData<GatewayCluster>> GetPagedListAsync(
            GatewayClusterQueryModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// 插入集群
        /// </summary>
        ValueTask<int> InsertAsync(GatewayCluster model);

        /// <summary>
        /// 更新集群
        /// </summary>
        ValueTask<int> UpdateAsync(GatewayCluster model);

        /// <summary>
        /// 删除集群
        /// </summary>
        ValueTask<int> DeleteAsync(Guid id);

        /// <summary>
        /// 检查ClusterId是否已存在
        /// </summary>
        Task<bool> ExistsByClusterIdAsync(string clusterId, Guid? excludeId = null);

        /// <summary>
        /// 检查集群是否被路由引用
        /// </summary>
        Task<bool> IsReferencedByRoutesAsync(string clusterId);
    }
}
