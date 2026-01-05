using System;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Mapping.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// Audit log application service implementation
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        public async Task<AuditLogDto?> GetAsync(Guid id)
        {
            var auditLog = await _unitOfWork.AuditLog.GetAsync(id);
            return auditLog?.ToDto();
        }

        /// <summary>
        /// Get paged audit log list
        /// </summary>
        public async Task<PagedData<AuditLogDto>> GetPagedListAsync(
            AuditLogModel model,
            int page,
            int itemsPerPage
        )
        {
            var pagedData = await _unitOfWork.AuditLog.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<AuditLogDto>
            {
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
                TotalCount = pagedData.TotalCount,
                Items = pagedData.Items.ToDtos(),
            };
        }

        /// <summary>
        /// Create audit log
        /// </summary>
        public async Task<AuditLogDto> CreateAsync(CreateAuditLogDto dto)
        {
            var auditLog = dto.ToEntity();

            await _unitOfWork.AuditLog.InsertAsync(auditLog);

            return auditLog.ToDto();
        }

        /// <summary>
        /// Delete expired audit logs
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime date)
        {
            return await _unitOfWork.AuditLog.DeleteOlderThanAsync(date);
        }
    }
}
