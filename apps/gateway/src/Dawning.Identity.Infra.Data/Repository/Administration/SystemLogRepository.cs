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
    /// System log repository implementation
    /// </summary>
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly DbContext _context;

        public SystemLogRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously get system log by ID
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
        /// Get paged system log list
        /// </summary>
        public async Task<PagedData<SystemLog>> GetPagedListAsync(
            SystemLogQueryModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<SystemLogEntity>(_context.Transaction);

            // Apply filter conditions
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

            // Order by created time descending (newest first)
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
        /// Asynchronously insert system log
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
        /// Batch delete expired system logs (data cleanup)
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
