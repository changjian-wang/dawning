using System;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// 审计日志仓储接口
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>
        /// 根据ID异步获取审计日志
        /// </summary>
        Task<AuditLog?> GetAsync(Guid id);

        /// <summary>
        /// 获取分页审计日志列表
        /// </summary>
        Task<PagedData<AuditLog>> GetPagedListAsync(
            AuditLogModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// 异步插入审计日志
        /// </summary>
        ValueTask<int> InsertAsync(AuditLog model);

        /// <summary>
        /// 批量删除过期的审计日志（数据清理）
        /// </summary>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
