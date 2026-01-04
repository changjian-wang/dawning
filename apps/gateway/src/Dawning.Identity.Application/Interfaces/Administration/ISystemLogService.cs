using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// System log service interface
    /// </summary>
    public interface ISystemLogService
    {
        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="exception">Exception object</param>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="statusCode">HTTP status code</param>
        Task LogErrorAsync(
            Exception exception,
            HttpContext? httpContext = null,
            int? statusCode = null
        );

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="httpContext">HTTP context</param>
        Task LogWarningAsync(string message, HttpContext? httpContext = null);

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="httpContext">HTTP context</param>
        Task LogInfoAsync(string message, HttpContext? httpContext = null);

        /// <summary>
        /// Create system log
        /// </summary>
        /// <param name="dto">Create system log DTO</param>
        Task<SystemLogDto> CreateAsync(CreateSystemLogDto dto);

        /// <summary>
        /// Get system log by ID
        /// </summary>
        /// <param name="id">Log ID</param>
        Task<SystemLogDto?> GetAsync(Guid id);

        /// <summary>
        /// Get paged system log list
        /// </summary>
        /// <param name="queryModel">Query model</param>
        /// <param name="page">Page number</param>
        /// <param name="itemsPerPage">Items per page</param>
        Task<PagedData<SystemLogDto>> GetPagedListAsync(
            SystemLogQueryModel queryModel,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// Delete logs before specified date
        /// </summary>
        /// <param name="beforeDate">Cutoff date</param>
        Task<int> DeleteOlderThanAsync(DateTime beforeDate);
    }
}
