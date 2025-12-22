using System;
using System.Threading.Tasks;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// 请求日志管理控制器
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/request-logs")]
    [Route("api/v{version:apiVersion}/request-logs")]
    [Authorize(Roles = "admin,super_admin,auditor")]
    public class RequestLogController : ControllerBase
    {
        private readonly IRequestLoggingService _requestLoggingService;
        private readonly ILogger<RequestLogController> _logger;

        public RequestLogController(
            IRequestLoggingService requestLoggingService,
            ILogger<RequestLogController> logger
        )
        {
            _requestLoggingService = requestLoggingService;
            _logger = logger;
        }

        /// <summary>
        /// 获取请求日志列表（分页）
        /// </summary>
        /// <param name="startTime">开始时间（UTC）</param>
        /// <param name="endTime">结束时间（UTC）</param>
        /// <param name="method">HTTP 方法过滤</param>
        /// <param name="path">路径过滤（模糊匹配）</param>
        /// <param name="statusCode">精确状态码</param>
        /// <param name="minStatusCode">最小状态码</param>
        /// <param name="maxStatusCode">最大状态码</param>
        /// <param name="userId">用户ID</param>
        /// <param name="clientIp">客户端IP</param>
        /// <param name="onlyErrors">只显示错误请求（状态码>=400）</param>
        /// <param name="slowRequestThresholdMs">慢请求阈值（毫秒）</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList(
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] string? method,
            [FromQuery] string? path,
            [FromQuery] int? statusCode,
            [FromQuery] int? minStatusCode,
            [FromQuery] int? maxStatusCode,
            [FromQuery] Guid? userId,
            [FromQuery] string? clientIp,
            [FromQuery] bool? onlyErrors,
            [FromQuery] long? slowRequestThresholdMs,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                // 验证参数
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var query = new RequestLogQuery
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Method = method?.ToUpperInvariant(),
                    Path = path,
                    StatusCode = statusCode,
                    MinStatusCode = minStatusCode,
                    MaxStatusCode = maxStatusCode,
                    UserId = userId,
                    ClientIp = clientIp,
                    OnlyErrors = onlyErrors,
                    SlowRequestThresholdMs = slowRequestThresholdMs,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _requestLoggingService.GetLogsAsync(query);

                return Ok(ApiResponse<PagedRequestLogs>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving request logs");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 获取请求统计信息
        /// </summary>
        /// <param name="startTime">开始时间（UTC），默认过去24小时</param>
        /// <param name="endTime">结束时间（UTC），默认当前时间</param>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime
        )
        {
            try
            {
                var result = await _requestLoggingService.GetStatisticsAsync(startTime, endTime);
                return Ok(ApiResponse<RequestStatistics>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving request statistics");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 清理过期日志
        /// </summary>
        /// <param name="retentionDays">保留天数（默认30天）</param>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "super_admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Cleanup([FromQuery] int retentionDays = 30)
        {
            try
            {
                if (retentionDays < 1)
                {
                    return BadRequest(ApiResponse.Error(40000, "Retention days must be at least 1"));
                }

                var deletedCount = await _requestLoggingService.CleanupOldLogsAsync(retentionDays);

                _logger.LogInformation(
                    "Cleaned up {DeletedCount} old request logs (retention: {RetentionDays} days)",
                    deletedCount,
                    retentionDays
                );

                return Ok(ApiResponse<object>.Success(new
                {
                    DeletedCount = deletedCount,
                    RetentionDays = retentionDays,
                    Message = $"Deleted {deletedCount} request logs older than {retentionDays} days"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old request logs");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 获取错误请求摘要
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        [HttpGet("errors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetErrors(
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var query = new RequestLogQuery
                {
                    StartTime = startTime ?? DateTime.UtcNow.AddDays(-1),
                    EndTime = endTime ?? DateTime.UtcNow,
                    OnlyErrors = true,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _requestLoggingService.GetLogsAsync(query);
                return Ok(ApiResponse<PagedRequestLogs>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving error logs");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 获取慢请求列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="thresholdMs">响应时间阈值（毫秒），默认1000ms</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        [HttpGet("slow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSlowRequests(
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] long thresholdMs = 1000,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;
                if (thresholdMs < 1) thresholdMs = 1000;

                var query = new RequestLogQuery
                {
                    StartTime = startTime ?? DateTime.UtcNow.AddDays(-1),
                    EndTime = endTime ?? DateTime.UtcNow,
                    SlowRequestThresholdMs = thresholdMs,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _requestLoggingService.GetLogsAsync(query);
                return Ok(ApiResponse<PagedRequestLogs>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving slow requests");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }
    }
}
