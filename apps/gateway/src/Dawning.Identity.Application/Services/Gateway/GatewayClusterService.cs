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
    /// Gateway cluster service implementation
    /// </summary>
    public class GatewayClusterService : IGatewayClusterService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GatewayClusterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get cluster by ID
        /// </summary>
        public async Task<GatewayClusterDto?> GetAsync(Guid id)
        {
            var cluster = await _unitOfWork.GatewayCluster.GetAsync(id);
            return cluster.ToDtoOrNull();
        }

        /// <summary>
        /// Get cluster by ClusterId
        /// </summary>
        public async Task<GatewayClusterDto?> GetByClusterIdAsync(string clusterId)
        {
            var cluster = await _unitOfWork.GatewayCluster.GetByClusterIdAsync(clusterId);
            return cluster.ToDtoOrNull();
        }

        /// <summary>
        /// Get all enabled clusters
        /// </summary>
        public async Task<IEnumerable<GatewayClusterDto>> GetAllEnabledAsync()
        {
            var clusters = await _unitOfWork.GatewayCluster.GetAllEnabledAsync();
            return clusters.ToDtos();
        }

        /// <summary>
        /// Get cluster options list
        /// </summary>
        public async Task<IEnumerable<ClusterOptionDto>> GetOptionsAsync()
        {
            var clusters = await _unitOfWork.GatewayCluster.GetAllAsync();
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
            var pagedData = await _unitOfWork.GatewayCluster.GetPagedListAsync(queryModel, page, pageSize);
            return new PagedData<GatewayClusterDto>
            {
                Items = pagedData.Items.ToDtos(),
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
            if (await _unitOfWork.GatewayCluster.ExistsByClusterIdAsync(dto.ClusterId))
            {
                throw new InvalidOperationException($"Cluster ID '{dto.ClusterId}' already exists");
            }

            var cluster = dto.ToEntity();
            cluster.CreatedBy = username;

            await _unitOfWork.GatewayCluster.InsertAsync(cluster);

            return cluster.ToDto();
        }

        /// <summary>
        /// Update cluster
        /// </summary>
        public async Task<GatewayClusterDto?> UpdateAsync(
            UpdateGatewayClusterDto dto,
            string? username = null
        )
        {
            var existing = await _unitOfWork.GatewayCluster.GetAsync(dto.Id);
            if (existing == null)
            {
                return null;
            }

            // Check if ClusterId is used by other records
            if (await _unitOfWork.GatewayCluster.ExistsByClusterIdAsync(dto.ClusterId, dto.Id))
            {
                throw new InvalidOperationException(
                    $"Cluster ID '{dto.ClusterId}' is already used by another cluster"
                );
            }

            // If ClusterId changes, check if routes reference it
            if (existing.ClusterId != dto.ClusterId)
            {
                if (await _unitOfWork.GatewayCluster.IsReferencedByRoutesAsync(existing.ClusterId))
                {
                    throw new InvalidOperationException(
                        $"Cluster '{existing.ClusterId}' is being referenced by routes, cannot change cluster ID"
                    );
                }
            }

            existing.ApplyUpdate(dto);
            existing.UpdatedBy = username;

            await _unitOfWork.GatewayCluster.UpdateAsync(existing);

            return existing.ToDto();
        }

        /// <summary>
        /// Delete cluster
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(Guid id)
        {
            var cluster = await _unitOfWork.GatewayCluster.GetAsync(id);
            if (cluster == null)
            {
                return (false, "Cluster not found");
            }

            // Check if routes reference this cluster
            if (await _unitOfWork.GatewayCluster.IsReferencedByRoutesAsync(cluster.ClusterId))
            {
                return (
                    false,
                    $"Cluster '{cluster.ClusterId}' is being referenced by routes, cannot be deleted"
                );
            }

            var result = await _unitOfWork.GatewayCluster.DeleteAsync(id);
            return (result > 0, null);
        }

        /// <summary>
        /// Toggle cluster enabled status
        /// </summary>
        public async Task<bool> ToggleEnabledAsync(Guid id, bool isEnabled, string? username = null)
        {
            var cluster = await _unitOfWork.GatewayCluster.GetAsync(id);
            if (cluster == null)
            {
                return false;
            }

            cluster.IsEnabled = isEnabled;
            cluster.UpdatedAt = DateTime.UtcNow;
            cluster.UpdatedBy = username;

            var result = await _unitOfWork.GatewayCluster.UpdateAsync(cluster);
            return result > 0;
        }
    }
}