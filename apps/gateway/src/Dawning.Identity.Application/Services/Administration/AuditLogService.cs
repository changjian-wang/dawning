using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
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
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AuditLogService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        public async Task<AuditLogDto?> GetAsync(Guid id)
        {
            var auditLog = await _uow.AuditLog.GetAsync(id);
            return auditLog != null ? _mapper.Map<AuditLogDto>(auditLog) : null;
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
            var pagedData = await _uow.AuditLog.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<AuditLogDto>
            {
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
                TotalCount = pagedData.TotalCount,
                Items = pagedData.Items.Select(a => _mapper.Map<AuditLogDto>(a)),
            };
        }

        /// <summary>
        /// Create audit log
        /// </summary>
        public async Task<AuditLogDto> CreateAsync(CreateAuditLogDto dto)
        {
            var auditLog = _mapper.Map<AuditLog>(dto);
            auditLog.Id = Guid.NewGuid();
            auditLog.CreatedAt = DateTime.UtcNow;

            await _uow.AuditLog.InsertAsync(auditLog);

            return _mapper.Map<AuditLogDto>(auditLog);
        }

        /// <summary>
        /// Delete expired audit logs
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime date)
        {
            return await _uow.AuditLog.DeleteOlderThanAsync(date);
        }
    }
}
