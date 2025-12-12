using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// 系统日志仓储接口
    /// </summary>
    public interface ISystemLogRepository
    {
        /// <summary>
        /// 根据ID异步获取系统日志
        /// </summary>
        Task<SystemLog?> GetAsync(Guid id);

        /// <summary>
        /// 获取分页系统日志列表
        /// </summary>
        Task<PagedData<SystemLog>> GetPagedListAsync(SystemLogQueryModel model, int page, int itemsPerPage);

        /// <summary>
        /// 异步插入系统日志
        /// </summary>
        ValueTask<int> InsertAsync(SystemLog model);

        /// <summary>
        /// 批量删除过期的系统日志（数据清理）
        /// </summary>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
