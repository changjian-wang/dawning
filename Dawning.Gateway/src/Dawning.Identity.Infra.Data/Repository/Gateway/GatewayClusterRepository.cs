using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Gateway;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;
using Dawning.Shared.Dapper.Contrib;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Gateway
{
    /// <summary>
    /// 网关集群仓储实现
    /// </summary>
    public class GatewayClusterRepository : IGatewayClusterRepository
    {
        private readonly DbContext _context;

        public GatewayClusterRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID获取集群
        /// </summary>
        public async Task<GatewayCluster?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<GatewayClusterEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// 根据ClusterId获取集群
        /// </summary>
        public async Task<GatewayCluster?> GetByClusterIdAsync(string clusterId)
        {
            var sql = "SELECT * FROM gateway_clusters WHERE cluster_id = @clusterId";
            var entity = await _context.Connection.QueryFirstOrDefaultAsync<GatewayClusterEntity>(
                sql,
                new { clusterId },
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// 获取所有启用的集群
        /// </summary>
        public async Task<IEnumerable<GatewayCluster>> GetAllEnabledAsync()
        {
            var sql = "SELECT * FROM gateway_clusters WHERE is_enabled = true ORDER BY cluster_id";
            var entities = await _context.Connection.QueryAsync<GatewayClusterEntity>(
                sql,
                transaction: _context.Transaction
            );
            return entities.ToModels();
        }

        /// <summary>
        /// 获取所有集群（用于下拉选择）
        /// </summary>
        public async Task<IEnumerable<GatewayCluster>> GetAllAsync()
        {
            var sql = "SELECT * FROM gateway_clusters ORDER BY cluster_id";
            var entities = await _context.Connection.QueryAsync<GatewayClusterEntity>(
                sql,
                transaction: _context.Transaction
            );
            return entities.ToModels();
        }

        /// <summary>
        /// 获取分页集群列表
        /// </summary>
        public async Task<PagedData<GatewayCluster>> GetPagedListAsync(
            GatewayClusterQueryModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<GatewayClusterEntity>(_context.Transaction);

            // 应用过滤条件
            builder = builder
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.ClusterId),
                    c => c.ClusterId.Contains(model.ClusterId ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    c => c.Name.Contains(model.Name ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.LoadBalancingPolicy),
                    c => c.LoadBalancingPolicy == model.LoadBalancingPolicy
                )
                .WhereIf(model.IsEnabled.HasValue, c => c.IsEnabled == model.IsEnabled!.Value);

            // 关键词搜索
            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                builder = builder.Where(c =>
                    c.ClusterId.Contains(model.Keyword) ||
                    c.Name.Contains(model.Keyword) ||
                    (c.Description != null && c.Description.Contains(model.Keyword))
                );
            }

            // 按ID排序
            var result = await builder
                .OrderBy(c => c.ClusterId)
                .AsPagedListAsync(page, itemsPerPage);

            return new PagedData<GatewayCluster>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };
        }

        /// <summary>
        /// 插入集群
        /// </summary>
        public async ValueTask<int> InsertAsync(GatewayCluster model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// 更新集群
        /// </summary>
        public async ValueTask<int> UpdateAsync(GatewayCluster model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;
            var success = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return success ? 1 : 0;
        }

        /// <summary>
        /// 删除集群
        /// </summary>
        public async ValueTask<int> DeleteAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<GatewayClusterEntity>(id, _context.Transaction);
            if (entity == null) return 0;

            var success = await _context.Connection.DeleteAsync(entity, _context.Transaction);
            return success ? 1 : 0;
        }

        /// <summary>
        /// 检查ClusterId是否已存在
        /// </summary>
        public async Task<bool> ExistsByClusterIdAsync(string clusterId, Guid? excludeId = null)
        {
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM gateway_clusters WHERE cluster_id = @clusterId AND id != @excludeId"
                : "SELECT COUNT(1) FROM gateway_clusters WHERE cluster_id = @clusterId";

            var count = await _context.Connection.ExecuteScalarAsync<int>(
                sql,
                new { clusterId, excludeId },
                _context.Transaction
            );
            return count > 0;
        }

        /// <summary>
        /// 检查集群是否被路由引用
        /// </summary>
        public async Task<bool> IsReferencedByRoutesAsync(string clusterId)
        {
            var sql = "SELECT COUNT(1) FROM gateway_routes WHERE cluster_id = @clusterId";
            var count = await _context.Connection.ExecuteScalarAsync<int>(
                sql,
                new { clusterId },
                _context.Transaction
            );
            return count > 0;
        }
    }
}
