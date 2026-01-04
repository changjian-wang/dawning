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
    /// Gateway cluster service implementation
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
        /// Get cluster by ID
        /// </summary>
        public async Task<GatewayClusterDto?> GetAsync(Guid id)
        {
            var cluster = await _uow.GatewayCluster.GetAsync(id);
            return cluster == null ? null : _mapper.Map<GatewayClusterDto>(cluster);
        }

        /// <summary>
        /// Get cluster by ClusterId
        /// </summary>
        public async Task<GatewayClusterDto?> GetByClusterIdAsync(string clusterId)
        {
            var cluster = await _uow.GatewayCluster.GetByClusterIdAsync(clusterId);
            return cluster == null ? null : _mapper.Map<GatewayClusterDto>(cluster);
        }

        /// <summary>
        /// Get all enabled clusters
        /// </summary>
        public async Task<IEnumerable<GatewayClusterDto>> GetAllEnabledAsync()
        {
            var clusters = await _uow.GatewayCluster.GetAllEnabledAsync();
            return _mapper.Map<IEnumerable<GatewayClusterDto>>(clusters);
        }

        /// <summary>
        /// Get cluster options list
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
        /// Get paged cluster list
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
        /// Create cluster
        /// </summary>
        public async Task<GatewayClusterDto> CreateAsync(
            CreateGatewayClusterDto dto,
            string? username = null
        )
        {
            // Check if ClusterId already exists
            if (await _uow.GatewayCluster.ExistsByClusterIdAsync(dto.ClusterId))
            {
                throw new InvalidOperationException($"Cluster ID '{dto.ClusterId}' already exists");
            }

            var cluster = _mapper.Map<GatewayCluster>(dto);
            cluster.Id = Guid.NewGuid();
            cluster.CreatedAt = DateTime.UtcNow;
            cluster.CreatedBy = username;

            await _uow.GatewayCluster.InsertAsync(cluster);

            return _mapper.Map<GatewayClusterDto>(cluster);
        }

        /// <summary>
        /// Update cluster
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

            // Check if ClusterId is used by other records
            if (await _uow.GatewayCluster.ExistsByClusterIdAsync(dto.ClusterId, dto.Id))
            {
                throw new InvalidOperationException($"Cluster ID '{dto.ClusterId}' is already used by another cluster");
            }

            // If ClusterId changes, check if routes reference it
            if (existing.ClusterId != dto.ClusterId)
            {
                if (await _uow.GatewayCluster.IsReferencedByRoutesAsync(existing.ClusterId))
                {
                    throw new InvalidOperationException(
                        $"Cluster '{existing.ClusterId}' is being referenced by routes, cannot change cluster ID"
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
        /// Delete cluster
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id)
        {
            var cluster = await _uow.GatewayCluster.GetAsync(id);
            if (cluster == null)
            {
                return (false, "Cluster not found");
            }

            // Check if routes reference this cluster
            if (await _uow.GatewayCluster.IsReferencedByRoutesAsync(cluster.ClusterId))
            {
                return (false, $"Cluster '{cluster.ClusterId}' is being referenced by routes, cannot be deleted");
            }

            var result = await _uow.GatewayCluster.DeleteAsync(id);
            return (result > 0, null);
        }

        /// <summary>
        /// Toggle cluster enabled status
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
