using System;
using System.Threading.Tasks;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// Audit log management controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/audit-log")]
    [Route("api/v{version:apiVersion}/audit-log")]
    [Authorize(Roles = "admin,super_admin,auditor")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(
            IAuditLogService auditLogService,
            ILogger<AuditLogController> logger
        )
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get audit log by ID
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
                    return NotFound(ApiResponse.Error(40400, "Audit log not found"));
                }

                return Ok(ApiResponse<object>.Success(auditLog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log by ID: {AuditLogId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// Get audit log list (paginated)
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
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                // Parse date strings to DateTime
                DateTime? parsedStartDate = null;
                DateTime? parsedEndDate = null;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateTime.TryParse(startDate, out var start))
                    {
                        parsedStartDate = start;
                    }
                    else
                    {
                        return BadRequest(
                            ApiResponse.Error(40000, $"Invalid startDate format: {startDate}")
                        );
                    }
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParse(endDate, out var end))
                    {
                        parsedEndDate = end;
                    }
                    else
                    {
                        return BadRequest(
                            ApiResponse.Error(40000, $"Invalid endDate format: {endDate}")
                        );
                    }
                }

                _logger.LogInformation(
                    "GetList called with parameters: userId={UserId}, username={Username}, action={Action}, "
                        + "entityType={EntityType}, entityId={EntityId}, ipAddress={IpAddress}, "
                        + "startDate={StartDate}, endDate={EndDate}, page={Page}, pageSize={PageSize}",
                    userId,
                    username,
                    action,
                    entityType,
                    entityId,
                    ipAddress,
                    parsedStartDate,
                    parsedEndDate,
                    page,
                    pageSize
                );

                var model = new AuditLogModel
                {
                    UserId = userId,
                    Username = username,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = ipAddress,
                    StartDate = parsedStartDate,
                    EndDate = parsedEndDate,
                };

                var result = await _auditLogService.GetPagedListAsync(model, page, pageSize);

                _logger.LogInformation(
                    "Audit log list retrieved: page {Page}, total {Total}",
                    page,
                    result.TotalCount
                );

                return Ok(
                    ApiResponse<object>.SuccessPaged(
                        result.Items,
                        result.PageIndex,
                        result.PageSize,
                        result.TotalCount
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log list: {Message}", ex.Message);
                return StatusCode(
                    500,
                    ApiResponse.Error(50000, $"Internal server error: {ex.Message}")
                );
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
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = auditLog.Id },
                    ApiResponse<object>.Success(auditLog)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
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

                return Ok(
                    ApiResponse<object>.Success(
                        new
                        {
                            deletedCount,
                            cutoffDate,
                            daysToKeep,
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old audit logs");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }
    }
}
