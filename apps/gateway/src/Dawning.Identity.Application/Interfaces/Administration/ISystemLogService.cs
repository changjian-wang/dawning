using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// 系统日志服务接口
    /// </summary>
    public interface ISystemLogService
    {
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <param name="httpContext">HTTP上下文</param>
        /// <param name="statusCode">HTTP状态码</param>
        Task LogErrorAsync(
            Exception exception,
            HttpContext? httpContext = null,
            int? statusCode = null
        );

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="httpContext">HTTP上下文</param>
        Task LogWarningAsync(string message, HttpContext? httpContext = null);

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="httpContext">HTTP上下文</param>
        Task LogInfoAsync(string message, HttpContext? httpContext = null);

        /// <summary>
        /// 创建系统日志
        /// </summary>
        /// <param name="dto">创建系统日志DTO</param>
        Task<SystemLogDto> CreateAsync(CreateSystemLogDto dto);

        /// <summary>
        /// 根据ID获取系统日志
        /// </summary>
        /// <param name="id">日志ID</param>
        Task<SystemLogDto?> GetAsync(Guid id);

        /// <summary>
        /// 分页获取系统日志列表
        /// </summary>
        /// <param name="queryModel">查询模型</param>
        /// <param name="page">页码</param>
        /// <param name="itemsPerPage">每页条数</param>
        Task<PagedData<SystemLogDto>> GetPagedListAsync(
            SystemLogQueryModel queryModel,
            int page,
            int itemsPerPage
        );

        /// <summary>
        /// 删除指定日期之前的日志
        /// </summary>
        /// <param name="beforeDate">截止日期</param>
        Task<int> DeleteOlderThanAsync(DateTime beforeDate);
    }
}
