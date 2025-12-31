using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// System log management controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class SystemLogController : ControllerBase
    {
        private readonly ISystemLogService _systemLogService;
        private readonly ILogger<SystemLogController> _logger;

        public SystemLogController(
            ISystemLogService systemLogService,
            ILogger<SystemLogController> logger
        )
        {
            _systemLogService = systemLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated system log list
        /// </summary>
        /// <param name="level">Log level (Information/Warning/Error)</param>
        /// <param name="keyword">Keyword search (Message/Exception)</param>
        /// <param name="userId">User ID</param>
        /// <param name="username">Username</param>
        /// <param name="ipAddress">IP address</param>
        /// <param name="requestPath">Request path</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Page size (default 20)</param>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] string? level = null,
            [FromQuery] string? keyword = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] string? username = null,
            [FromQuery] string? ipAddress = null,
            [FromQuery] string? requestPath = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                var queryModel = new SystemLogQueryModel
                {
                    Level = level,
                    Keyword = keyword,
                    UserId = userId,
                    Username = username,
                    IpAddress = ipAddress,
                    RequestPath = requestPath,
                    StartDate = startDate,
                    EndDate = endDate,
                };

                var result = await _systemLogService.GetPagedListAsync(queryModel, page, pageSize);

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged system logs");
                return StatusCode(
                    500,
                    new
                    {
                        code = 50000,
                        message = "Failed to retrieve system logs",
                        error = ex.Message,
                    }
                );
            }
        }

        /// <summary>
        /// 根据ID获取系统日志详情
        /// </summary>
        /// <param name="id">日志ID</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var log = await _systemLogService.GetAsync(id);

                if (log == null)
                {
                    return NotFound(new { code = 40400, message = "System log not found" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = log,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system log {Id}", id);
                return StatusCode(
                    500,
                    new
                    {
                        code = 50000,
                        message = "Failed to retrieve system log",
                        error = ex.Message,
                    }
                );
            }
        }

        /// <summary>
        /// 删除指定日期之前的日志（超级管理员功能）
        /// </summary>
        /// <param name="beforeDate">截止日期</param>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "super_admin")]
        public async Task<IActionResult> Cleanup([FromQuery] DateTime beforeDate)
        {
            try
            {
                var count = await _systemLogService.DeleteOlderThanAsync(beforeDate);

                return Ok(
                    new
                    {
                        code = 20000,
                        message = $"Successfully deleted {count} log entries",
                        data = new { deletedCount = count },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cleaning up system logs before {BeforeDate}",
                    beforeDate
                );
                return StatusCode(
                    500,
                    new
                    {
                        code = 50000,
                        message = "Failed to cleanup system logs",
                        error = ex.Message,
                    }
                );
            }
        }

        /// <summary>
        /// 手动记录日志（测试用）
        /// </summary>
        [HttpPost("test")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestLog([FromBody] CreateSystemLogDto dto)
        {
            try
            {
                var log = await _systemLogService.CreateAsync(dto);

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Log created successfully",
                        data = log,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test log");
                return StatusCode(
                    500,
                    new
                    {
                        code = 50000,
                        message = "Failed to create log",
                        error = ex.Message,
                    }
                );
            }
        }
    }
}
