using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// Audit log application service interface
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Get audit log by ID
        /// </summary>
        Task<AuditLogDto?> GetAsync(Guid id);

        /// <summary>
        /// Get paged audit log list
        /// </summary>
        Task<PagedData<AuditLogDto>> GetPagedListAsync(
            AuditLogModel model,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// Create audit log
        /// </summary>
        Task<AuditLogDto> CreateAsync(CreateAuditLogDto dto);

        /// <summary>
        /// Delete expired audit logs
        /// </summary>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
