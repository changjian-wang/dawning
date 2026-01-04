using System;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// System log repository interface
    /// </summary>
    public interface ISystemLogRepository
    {
        /// <summary>
        /// Asynchronously get system log by ID
        /// </summary>
        Task<SystemLog?> GetAsync(Guid id);

        /// <summary>
        /// Get paginated system log list
        /// </summary>
        Task<PagedData<SystemLog>> GetPagedListAsync(
            SystemLogQueryModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// Asynchronously insert system log
        /// </summary>
        ValueTask<int> InsertAsync(SystemLog model);

        /// <summary>
        /// Batch delete expired system logs (data cleanup)
        /// </summary>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
