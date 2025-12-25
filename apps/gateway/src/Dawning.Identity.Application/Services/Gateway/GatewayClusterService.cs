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
    /// 网关集群服务实现
    /// </summary>
    public class GatewayClusterService : IGatewayClusterService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GatewayClusterService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// 根据ID获取集群
        /// </summary>
        public async Task<GatewayClusterDto?> GetAsync(Guid id)
        {
            var cluster = await _uow.GatewayCluster.GetAsync(id);
            return cluster == null ? null : _mapper.Map<GatewayClusterDto>(cluster);
        }

        /// <summary>
        /// 根据ClusterId获取集群
        /// </summary>
        public async Task<GatewayClusterDto?> GetByClusterIdAsync(string clusterId)
        {
            var cluster = await _uow.GatewayCluster.GetByClusterIdAsync(clusterId);
            return cluster == null ? null : _mapper.Map<GatewayClusterDto>(cluster);
        }

        /// <summary>
        /// 获取所有启用的集群
        /// </summary>
        public async Task<IEnumerable<GatewayClusterDto>> GetAllEnabledAsync()
        {
            var clusters = await _uow.GatewayCluster.GetAllEnabledAsync();
            return _mapper.Map<IEnumerable<GatewayClusterDto>>(clusters);
        }

        /// <summary>
        /// 获取集群选项列表
        /// </summary>
        public async Task<IEnumerable<ClusterOptionDto>> GetOptionsAsync()
        {
            var clusters = await _uow.GatewayCluster.GetAllAsync();
            return clusters.Select(c => new ClusterOptionDto
            {
                ClusterId = c.ClusterId,
                Name = c.Name,
                IsEnabled = c.IsEnabled,
            });
        }

        /// <summary>
        /// 获取分页集群列表
        /// </summary>
        public async Task<PagedData<GatewayClusterDto>> GetPagedListAsync(
            GatewayClusterQueryModel queryModel,
            int page,
            int pageSize
        )
        {
            var pagedData = await _uow.GatewayCluster.GetPagedListAsync(queryModel, page, pageSize);
            return new PagedData<GatewayClusterDto>
            {
                Items = _mapper.Map<IEnumerable<GatewayClusterDto>>(pagedData.Items),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
            };
        }

        /// <summary>
        /// 创建集群
        /// </summary>
        public async Task<GatewayClusterDto> CreateAsync(
            CreateGatewayClusterDto dto,
            string? username = null
        )
        {
            // 检查ClusterId是否已存在
            if (await _uow.GatewayCluster.ExistsByClusterIdAsync(dto.ClusterId))
            {
                throw new InvalidOperationException($"集群ID '{dto.ClusterId}' 已存在");
            }

            var cluster = _mapper.Map<GatewayCluster>(dto);
            cluster.Id = Guid.NewGuid();
            cluster.CreatedAt = DateTime.UtcNow;
            cluster.CreatedBy = username;

            await _uow.GatewayCluster.InsertAsync(cluster);

            return _mapper.Map<GatewayClusterDto>(cluster);
        }

        /// <summary>
        /// 更新集群
        /// </summary>
        public async Task<GatewayClusterDto?> UpdateAsync(
            UpdateGatewayClusterDto dto,
            string? username = null
        )
        {
            var existing = await _uow.GatewayCluster.GetAsync(dto.Id);
            if (existing == null)
            {
                return null;
            }

            // 检查ClusterId是否被其他记录使用
            if (await _uow.GatewayCluster.ExistsByClusterIdAsync(dto.ClusterId, dto.Id))
            {
                throw new InvalidOperationException($"集群ID '{dto.ClusterId}' 已被其他集群使用");
            }

            // 如果ClusterId发生变化，需要检查是否有路由引用
            if (existing.ClusterId != dto.ClusterId)
            {
                if (await _uow.GatewayCluster.IsReferencedByRoutesAsync(existing.ClusterId))
                {
                    throw new InvalidOperationException(
                        $"集群 '{existing.ClusterId}' 正在被路由引用，无法更改集群ID"
                    );
                }
            }

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = username;

            await _uow.GatewayCluster.UpdateAsync(existing);

            return _mapper.Map<GatewayClusterDto>(existing);
        }

        /// <summary>
        /// 删除集群
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id)
        {
            var cluster = await _uow.GatewayCluster.GetAsync(id);
            if (cluster == null)
            {
                return (false, "集群不存在");
            }

            // 检查是否有路由引用
            if (await _uow.GatewayCluster.IsReferencedByRoutesAsync(cluster.ClusterId))
            {
                return (false, $"集群 '{cluster.ClusterId}' 正在被路由引用，无法删除");
            }

            var result = await _uow.GatewayCluster.DeleteAsync(id);
            return (result > 0, null);
        }

        /// <summary>
        /// 切换集群启用状态
        /// </summary>
        public async Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null)
        {
            var cluster = await _uow.GatewayCluster.GetAsync(id);
            if (cluster == null)
            {
                return false;
            }

            cluster.IsEnabled = isEnabled;
            cluster.UpdatedAt = DateTime.UtcNow;
            cluster.UpdatedBy = username;

            var result = await _uow.GatewayCluster.UpdateAsync(cluster);
            return result > 0;
        }
    }
}
