using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// 审计日志管理控制器
    /// </summary>
    [ApiController]
    [Route("api/audit-log")]
    [Authorize(Roles = "admin,super_admin,auditor")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// 根据ID获取审计日志
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var auditLog = await _auditLogService.GetAsync(id);
                if (auditLog == null)
                {
                    return NotFound(new { code = 404, message = "Audit log not found" });
                }

                return Ok(new { code = 0, message = "Success", data = auditLog });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log by ID: {AuditLogId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 获取审计日志列表（分页）
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList(
            [FromQuery] Guid? userId,
            [FromQuery] string? username,
            [FromQuery] string? action,
            [FromQuery] string? entityType,
            [FromQuery] Guid? entityId,
            [FromQuery] string? ipAddress,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var model = new AuditLogModel
                {
                    UserId = userId,
                    Username = username,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = ipAddress,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var result = await _auditLogService.GetPagedListAsync(model, page, pageSize);

                _logger.LogInformation("Audit log list retrieved: page {Page}, total {Total}", page, result.TotalCount);

                return Ok(new
                {
                    code = 0,
                    message = "Success",
                    data = new
                    {
                        list = result.Items,
                        pagination = new
                        {
                            current = result.PageIndex,
                            pageSize = result.PageSize,
                            total = result.TotalCount
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log list");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 创建审计日志（内部使用）
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status201Created)]
        [Authorize(Roles = "super_admin")] // 只有超级管理员可以手动创建
        public async Task<IActionResult> Create([FromBody] CreateAuditLogDto dto)
        {
            try
            {
                var auditLog = await _auditLogService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = auditLog.Id }, 
                    new { code = 0, message = "Success", data = auditLog });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 删除过期的审计日志（数据清理）
        /// </summary>
        [HttpDelete("cleanup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "super_admin")]
        public async Task<IActionResult> CleanupOldLogs([FromQuery] int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var deletedCount = await _auditLogService.DeleteOlderThanAsync(cutoffDate);

                return Ok(new
                {
                    code = 0,
                    message = "Success",
                    data = new { deletedCount, cutoffDate, daysToKeep }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old audit logs");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }
    }
}
