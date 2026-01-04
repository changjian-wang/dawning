using System;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// Audit log repository interface
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>
        /// Asynchronously get audit log by ID
        /// </summary>
        Task<AuditLog?> GetAsync(Guid id);

        /// <summary>
        /// Get paginated audit log list
        /// </summary>
        Task<PagedData<AuditLog>> GetPagedListAsync(
            AuditLogModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// Asynchronously insert audit log
        /// </summary>
        ValueTask<int> InsertAsync(AuditLog model);

        /// <summary>
        /// Batch delete expired audit logs (data cleanup)
        /// </summary>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
