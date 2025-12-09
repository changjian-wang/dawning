using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// 审计日志应用服务接口
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// 根据ID获取审计日志
        /// </summary>
        Task<AuditLogDto?> GetAsync(Guid id);

        /// <summary>
        /// 获取分页审计日志列表
        /// </summary>
        Task<PagedData<AuditLogDto>> GetPagedListAsync(AuditLogModel model, int page, int itemsPerPage);

        /// <summary>
        /// 创建审计日志
        /// </summary>
        Task<AuditLogDto> CreateAsync(CreateAuditLogDto dto);

        /// <summary>
        /// 删除过期的审计日志
        /// </summary>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
