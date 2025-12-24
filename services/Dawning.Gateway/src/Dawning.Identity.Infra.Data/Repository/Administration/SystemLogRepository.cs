using System;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.ORM.Dapper;
using static Dawning.ORM.Dapper.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// 系统日志仓储实现
    /// </summary>
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly DbContext _context;

        public SystemLogRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID异步获取系统日志
        /// </summary>
        public async Task<SystemLog?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<SystemLogEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// 获取分页系统日志列表
        /// </summary>
        public async Task<PagedData<SystemLog>> GetPagedListAsync(
            SystemLogQueryModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<SystemLogEntity>(_context.Transaction);

            // 应用过滤条件
            builder = builder
                .WhereIf(!string.IsNullOrWhiteSpace(model.Level), l => l.Level == model.Level)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Keyword),
                    l => l.Message.Contains(model.Keyword ?? "")
                )
                .WhereIf(model.UserId.HasValue, l => l.UserId == model.UserId!.Value)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Username),
                    l => l.Username!.Contains(model.Username ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.IpAddress),
                    l => l.IpAddress == model.IpAddress
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.RequestPath),
                    l => l.RequestPath!.Contains(model.RequestPath ?? "")
                )
                .WhereIf(model.StartDate.HasValue, l => l.CreatedAt >= model.StartDate!.Value)
                .WhereIf(model.EndDate.HasValue, l => l.CreatedAt <= model.EndDate!.Value);

            // 按创建时间降序排序（最新的在前）
            var result = await builder
                .OrderByDescending(l => l.CreatedAt)
                .AsPagedListAsync(page, itemsPerPage);

            return new PagedData<SystemLog>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };
        }

        /// <summary>
        /// 异步插入系统日志
        /// </summary>
        public async ValueTask<int> InsertAsync(SystemLog model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 批量删除过期的系统日志（数据清理）
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime date)
        {
            const string sql = "DELETE FROM system_logs WHERE created_at < @Date";
            var result = await _context.Connection.ExecuteAsync(
                sql,
                new { Date = date },
                _context.Transaction
            );
            return result;
        }
    }
}
