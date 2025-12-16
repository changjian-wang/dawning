using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;

namespace Dawning.Identity.Application.Interfaces.Gateway
{
    /// <summary>
    /// 网关集群服务接口
    /// </summary>
    public interface IGatewayClusterService
    {
        /// <summary>
        /// 根据ID获取集群
        /// </summary>
        Task<GatewayClusterDto?> GetAsync(Guid id);

        /// <summary>
        /// 根据ClusterId获取集群
        /// </summary>
        Task<GatewayClusterDto?> GetByClusterIdAsync(string clusterId);

        /// <summary>
        /// 获取所有启用的集群（用于YARP配置）
        /// </summary>
        Task<IEnumerable<GatewayClusterDto>> GetAllEnabledAsync();

        /// <summary>
        /// 获取集群选项列表（用于下拉选择）
        /// </summary>
        Task<IEnumerable<ClusterOptionDto>> GetOptionsAsync();

        /// <summary>
        /// 获取分页集群列表
        /// </summary>
        Task<PagedData<GatewayClusterDto>> GetPagedListAsync(
            GatewayClusterQueryModel queryModel,
            int page,
            int pageSize
        );

        /// <summary>
        /// 创建集群
        /// </summary>
        Task<GatewayClusterDto> CreateAsync(CreateGatewayClusterDto dto, string? username = null);

        /// <summary>
        /// 更新集群
        /// </summary>
        Task<GatewayClusterDto?> UpdateAsync(UpdateGatewayClusterDto dto, string? username = null);

        /// <summary>
        /// 删除集群
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id);

        /// <summary>
        /// 切换集群启用状态
        /// </summary>
        Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null);
    }
}
