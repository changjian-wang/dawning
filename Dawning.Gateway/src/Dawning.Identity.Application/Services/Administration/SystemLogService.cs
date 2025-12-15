using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// 系统日志服务实现
    /// </summary>
    public class SystemLogService : ISystemLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SystemLogService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public async Task LogErrorAsync(
            Exception exception,
            HttpContext? httpContext = null,
            int? statusCode = null
        )
        {
            var log = CreateLogFromException(exception, httpContext, statusCode, "Error");
            await _uow.SystemLog.InsertAsync(log);
            await _uow.CommitAsync();
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public async Task LogWarningAsync(string message, HttpContext? httpContext = null)
        {
            var log = CreateLogFromMessage(message, httpContext, "Warning");
            await _uow.SystemLog.InsertAsync(log);
            await _uow.CommitAsync();
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public async Task LogInfoAsync(string message, HttpContext? httpContext = null)
        {
            var log = CreateLogFromMessage(message, httpContext, "Information");
            await _uow.SystemLog.InsertAsync(log);
            await _uow.CommitAsync();
        }

        /// <summary>
        /// 创建系统日志
        /// </summary>
        public async Task<SystemLogDto> CreateAsync(CreateSystemLogDto dto)
        {
            var log = _mapper.Map<SystemLog>(dto);
            await _uow.SystemLog.InsertAsync(log);
            await _uow.CommitAsync();
            return _mapper.Map<SystemLogDto>(log);
        }

        /// <summary>
        /// 根据ID获取系统日志
        /// </summary>
        public async Task<SystemLogDto?> GetAsync(Guid id)
        {
            var log = await _uow.SystemLog.GetAsync(id);
            return _mapper.Map<SystemLogDto>(log);
        }

        /// <summary>
        /// 分页获取系统日志列表
        /// </summary>
        public async Task<PagedData<SystemLogDto>> GetPagedListAsync(
            SystemLogQueryModel queryModel,
            int page,
            int itemsPerPage
        )
        {
            var pagedData = await _uow.SystemLog.GetPagedListAsync(queryModel, page, itemsPerPage);

            return new PagedData<SystemLogDto>
            {
                Items = _mapper.Map<SystemLogDto[]>(pagedData.Items),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
            };
        }

        /// <summary>
        /// 删除指定日期之前的日志
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime beforeDate)
        {
            var count = await _uow.SystemLog.DeleteOlderThanAsync(beforeDate);
            await _uow.CommitAsync();
            return count;
        }

        #region Private Methods

        /// <summary>
        /// 从异常创建日志对象
        /// </summary>
        private SystemLog CreateLogFromException(
            Exception exception,
            HttpContext? httpContext,
            int? statusCode,
            string level
        )
        {
            var log = new SystemLog
            {
                Id = Guid.NewGuid(),
                Level = level,
                Message = exception.Message,
                Exception = exception.ToString(),
                StackTrace = exception.StackTrace,
                Source = exception.Source,
                CreatedAt = DateTime.UtcNow,
            };

            if (httpContext != null)
            {
                EnrichLogFromHttpContext(log, httpContext, statusCode);
            }

            return log;
        }

        /// <summary>
        /// 从消息创建日志对象
        /// </summary>
        private SystemLog CreateLogFromMessage(
            string message,
            HttpContext? httpContext,
            string level
        )
        {
            var log = new SystemLog
            {
                Id = Guid.NewGuid(),
                Level = level,
                Message = message,
                CreatedAt = DateTime.UtcNow,
            };

            if (httpContext != null)
            {
                EnrichLogFromHttpContext(log, httpContext, null);
            }

            return log;
        }

        /// <summary>
        /// 从HTTP上下文补充日志信息
        /// </summary>
        private void EnrichLogFromHttpContext(
            SystemLog log,
            HttpContext httpContext,
            int? statusCode
        )
        {
            // 用户信息
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = httpContext.User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier || c.Type == "sub"
                );
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    log.UserId = userId;
                }

                var usernameClaim = httpContext.User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "name"
                );
                log.Username = usernameClaim?.Value;
            }

            // IP地址
            log.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // User-Agent
            if (httpContext.Request.Headers.ContainsKey("User-Agent"))
            {
                log.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
            }

            // 请求路径和方法
            log.RequestPath = httpContext.Request.Path.Value;
            log.RequestMethod = httpContext.Request.Method;

            // HTTP状态码
            log.StatusCode = statusCode ?? httpContext.Response.StatusCode;
        }

        #endregion
    }
}
