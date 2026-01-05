using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Notification;
using Dawning.Identity.Application.Mapping.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// System log service implementation
    /// </summary>
    public class SystemLogService : ISystemLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealTimeNotificationService _realTimeNotification;
        private readonly ILogger<SystemLogService> _logger;

        public SystemLogService(
            IUnitOfWork unitOfWork,
            IRealTimeNotificationService realTimeNotification,
            ILogger<SystemLogService> logger
        )
        {
            _unitOfWork = unitOfWork;
            _realTimeNotification = realTimeNotification;
            _logger = logger;
        }

        /// <summary>
        /// Log an error
        /// </summary>
        public async Task LogErrorAsync(
            Exception exception,
            HttpContext? httpContext = null,
            int? statusCode = null
        )
        {
            var log = CreateLogFromException(exception, httpContext, statusCode, "Error");
            await _unitOfWork.SystemLog.InsertAsync(log);
            await _unitOfWork.CommitAsync();

            // Push error log in real-time
            await PushLogToClientsAsync(log);
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        public async Task LogWarningAsync(string message, HttpContext? httpContext = null)
        {
            var log = CreateLogFromMessage(message, httpContext, "Warning");
            await _unitOfWork.SystemLog.InsertAsync(log);
            await _unitOfWork.CommitAsync();

            // Push warning log in real-time
            await PushLogToClientsAsync(log);
        }

        /// <summary>
        /// Log an information message
        /// </summary>
        public async Task LogInfoAsync(string message, HttpContext? httpContext = null)
        {
            var log = CreateLogFromMessage(message, httpContext, "Information");
            await _unitOfWork.SystemLog.InsertAsync(log);
            await _unitOfWork.CommitAsync();

            // Only push important info logs (avoid log flooding)
            // Info logs are not pushed for now, can be enabled via configuration
        }

        /// <summary>
        /// Create a system log
        /// </summary>
        public async Task<SystemLogDto> CreateAsync(CreateSystemLogDto dto)
        {
            var log = dto.ToEntity();
            await _unitOfWork.SystemLog.InsertAsync(log);
            await _unitOfWork.CommitAsync();
            return log.ToDto();
        }

        /// <summary>
        /// Get system log by ID
        /// </summary>
        public async Task<SystemLogDto?> GetAsync(Guid id)
        {
            var log = await _unitOfWork.SystemLog.GetAsync(id);
            return log.ToDtoOrNull();
        }

        /// <summary>
        /// Get paged list of system logs
        /// </summary>
        public async Task<PagedData<SystemLogDto>> GetPagedListAsync(
            SystemLogQueryModel queryModel,
            int page,
            int itemsPerPage
        )
        {
            var pagedData = await _unitOfWork.SystemLog.GetPagedListAsync(
                queryModel,
                page,
                itemsPerPage
            );

            return new PagedData<SystemLogDto>
            {
                Items = pagedData.Items.ToDtoArray(),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
            };
        }

        /// <summary>
        /// Delete logs older than the specified date
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(DateTime beforeDate)
        {
            return await _unitOfWork.SystemLog.DeleteOlderThanAsync(beforeDate);
        }

        #region Private Methods

        /// <summary>
        /// Create log object from exception
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
        /// Create log object from message
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
        /// Enrich log information from HTTP context
        /// </summary>
        private void EnrichLogFromHttpContext(
            SystemLog log,
            HttpContext httpContext,
            int? statusCode
        )
        {
            // User information
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

            // IP address
            log.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // User-Agent
            if (httpContext.Request.Headers.ContainsKey("User-Agent"))
            {
                log.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
            }

            // Request path and method
            log.RequestPath = httpContext.Request.Path.Value;
            log.RequestMethod = httpContext.Request.Method;

            // HTTP status code
            log.StatusCode = statusCode ?? httpContext.Response.StatusCode;
        }

        /// <summary>
        /// Push log to real-time subscribed clients
        /// </summary>
        private async Task PushLogToClientsAsync(SystemLog log)
        {
            try
            {
                var logEntry = new RealTimeLogEntry
                {
                    Id = log.Id.ToString(),
                    Timestamp = log.CreatedAt,
                    Level = log.Level ?? "Information",
                    Message = log.Message ?? string.Empty,
                    Exception = log.Exception,
                    RequestPath = log.RequestPath,
                    RequestMethod = log.RequestMethod,
                    StatusCode = log.StatusCode,
                    UserId = log.UserId?.ToString(),
                    Username = log.Username,
                    IpAddress = log.IpAddress,
                };

                await _realTimeNotification.SendLogEntryAsync(logEntry);
            }
            catch (Exception ex)
            {
                // Push failure should not affect log recording
                _logger.LogDebug(ex, "Failed to push log in real-time");
            }
        }

        #endregion
    }
}
