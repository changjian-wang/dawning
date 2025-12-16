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
    /// 网关路由仓储实现
    /// </summary>
    public class GatewayRouteRepository : IGatewayRouteRepository
    {
        private readonly DbContext _context;

        public GatewayRouteRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID获取路由
        /// </summary>
        public async Task<GatewayRoute?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<GatewayRouteEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// 根据RouteId获取路由
        /// </summary>
        public async Task<GatewayRoute?> GetByRouteIdAsync(string routeId)
        {
            var sql = "SELECT * FROM gateway_routes WHERE route_id = @routeId";
            var entity = await _context.Connection.QueryFirstOrDefaultAsync<GatewayRouteEntity>(
                sql,
                new { routeId },
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// 获取所有启用的路由
        /// </summary>
        public async Task<IEnumerable<GatewayRoute>> GetAllEnabledAsync()
        {
            var sql = "SELECT * FROM gateway_routes WHERE is_enabled = true ORDER BY \"order\", route_id";
            var entities = await _context.Connection.QueryAsync<GatewayRouteEntity>(
                sql,
                transaction: _context.Transaction
            );
            return entities.ToModels();
        }

        /// <summary>
        /// 获取分页路由列表
        /// </summary>
        public async Task<PagedData<GatewayRoute>> GetPagedListAsync(
            GatewayRouteQueryModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<GatewayRouteEntity>(_context.Transaction);

            // 应用过滤条件
            builder = builder
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.RouteId),
                    r => r.RouteId.Contains(model.RouteId ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    r => r.Name.Contains(model.Name ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.ClusterId),
                    r => r.ClusterId == model.ClusterId
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.MatchPath),
                    r => r.MatchPath.Contains(model.MatchPath ?? "")
                )
                .WhereIf(model.IsEnabled.HasValue, r => r.IsEnabled == model.IsEnabled!.Value);

            // 关键词搜索
            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                builder = builder.Where(r =>
                    r.RouteId.Contains(model.Keyword) ||
                    r.Name.Contains(model.Keyword) ||
                    (r.Description != null && r.Description.Contains(model.Keyword)) ||
                    r.MatchPath.Contains(model.Keyword)
                );
            }

            // 按顺序和ID排序
            var result = await builder
                .OrderBy(r => r.Order)
                .ThenBy(r => r.RouteId)
                .AsPagedListAsync(page, itemsPerPage);

            return new PagedData<GatewayRoute>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };
        }

        /// <summary>
        /// 插入路由
        /// </summary>
        public async ValueTask<int> InsertAsync(GatewayRoute model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// 更新路由
        /// </summary>
        public async ValueTask<int> UpdateAsync(GatewayRoute model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;
            var success = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return success ? 1 : 0;
        }

        /// <summary>
        /// 删除路由
        /// </summary>
        public async ValueTask<int> DeleteAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<GatewayRouteEntity>(id, _context.Transaction);
            if (entity == null) return 0;

            var success = await _context.Connection.DeleteAsync(entity, _context.Transaction);
            return success ? 1 : 0;
        }

        /// <summary>
        /// 检查RouteId是否已存在
        /// </summary>
        public async Task<bool> ExistsByRouteIdAsync(string routeId, Guid? excludeId = null)
        {
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM gateway_routes WHERE route_id = @routeId AND id != @excludeId"
                : "SELECT COUNT(1) FROM gateway_routes WHERE route_id = @routeId";

            var count = await _context.Connection.ExecuteScalarAsync<int>(
                sql,
                new { routeId, excludeId },
                _context.Transaction
            );
            return count > 0;
        }
    }
}
