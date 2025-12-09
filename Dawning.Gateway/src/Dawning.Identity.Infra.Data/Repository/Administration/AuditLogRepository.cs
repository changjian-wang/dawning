using Dapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;
using System;
using System.Threading.Tasks;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// 审计日志仓储实现
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DbContext _context;

        public AuditLogRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID异步获取审计日志
        /// </summary>
        public async Task<AuditLog?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<AuditLogEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        /// <summary>
        /// 获取分页审计日志列表
        /// </summary>
        public async Task<PagedData<AuditLog>> GetPagedListAsync(AuditLogModel model, int page, int itemsPerPage)
        {
            var builder = _context.Connection.Builder<AuditLogEntity>(_context.Transaction);

            // 应用过滤条件
            builder = builder
                .WhereIf(model.UserId.HasValue, a => a.UserId == model.UserId!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Username), a => a.Username!.Contains(model.Username ?? ""))
                .WhereIf(!string.IsNullOrWhiteSpace(model.Action), a => a.Action == model.Action)
                .WhereIf(!string.IsNullOrWhiteSpace(model.EntityType), a => a.EntityType == model.EntityType)
                .WhereIf(model.EntityId.HasValue, a => a.EntityId == model.EntityId!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(model.IpAddress), a => a.IpAddress == model.IpAddress)
                .WhereIf(model.StartDate.HasValue, a => a.CreatedAt >= model.StartDate!.Value)
                .WhereIf(model.EndDate.HasValue, a => a.CreatedAt <= model.EndDate!.Value);

            // 按创建时间降序排序（最新的在前）
            var result = await builder
                .OrderByDescending(a => a.CreatedAt)
                .AsPagedListAsync(page, itemsPerPage);

            return new PagedData<AuditLog>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels()
            };
        }

        /// <summary>
        /// 异步插入审计日志
        /// </summary>
        public async ValueTask<int> InsertAsync(AuditLog model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 批量删除过期的审计日志（数据清理）
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime date)
        {
            var sql = "DELETE FROM audit_logs WHERE created_at < @Date";
            var result = await _context.Connection.ExecuteAsync(sql, new { Date = date }, _context.Transaction);
            return result;
        }
    }
}
