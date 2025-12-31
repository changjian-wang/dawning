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
    /// Request Log Management Controller
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
        /// Get request log list (paged)
        /// </summary>
        /// <param name="startTime">Start time (UTC)</param>
        /// <param name="endTime">End time (UTC)</param>
        /// <param name="method">HTTP method filter</param>
        /// <param name="path">Path filter (fuzzy match)</param>
        /// <param name="statusCode">Exact status code</param>
        /// <param name="minStatusCode">Minimum status code</param>
        /// <param name="maxStatusCode">Maximum status code</param>
        /// <param name="userId">User ID</param>
        /// <param name="clientIp">Client IP</param>
        /// <param name="onlyErrors">Show only error requests (status code >= 400)</param>
        /// <param name="slowRequestThresholdMs">Slow request threshold (milliseconds)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
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
                // Validate parameters
                if (page < 1)
                    page = 1;
                if (pageSize < 1)
                    pageSize = 20;
                if (pageSize > 100)
                    pageSize = 100;

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
                    PageSize = pageSize,
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
        /// Get request statistics
        /// </summary>
        /// <param name="startTime">Start time (UTC), default past 24 hours</param>
        /// <param name="endTime">End time (UTC), default current time</param>
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
        /// Clean up expired logs
        /// </summary>
        /// <param name="retentionDays">Retention days (default 30 days)</param>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "super_admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Cleanup([FromQuery] int retentionDays = 30)
        {
            try
            {
                if (retentionDays < 1)
                {
                    return BadRequest(
                        ApiResponse.Error(40000, "Retention days must be at least 1")
                    );
                }

                var deletedCount = await _requestLoggingService.CleanupOldLogsAsync(retentionDays);

                _logger.LogInformation(
                    "Cleaned up {DeletedCount} old request logs (retention: {RetentionDays} days)",
                    deletedCount,
                    retentionDays
                );

                return Ok(
                    ApiResponse<object>.Success(
                        new
                        {
                            DeletedCount = deletedCount,
                            RetentionDays = retentionDays,
                            Message = $"Deleted {deletedCount} request logs older than {retentionDays} days",
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old request logs");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// Get error request summary
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
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
                if (page < 1)
                    page = 1;
                if (pageSize < 1)
                    pageSize = 20;
                if (pageSize > 100)
                    pageSize = 100;

                var query = new RequestLogQuery
                {
                    StartTime = startTime ?? DateTime.UtcNow.AddDays(-1),
                    EndTime = endTime ?? DateTime.UtcNow,
                    OnlyErrors = true,
                    Page = page,
                    PageSize = pageSize,
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
        /// Get slow request list
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="thresholdMs">Response time threshold (milliseconds), default 1000ms</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
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
                if (page < 1)
                    page = 1;
                if (pageSize < 1)
                    pageSize = 20;
                if (pageSize > 100)
                    pageSize = 100;
                if (thresholdMs < 1)
                    thresholdMs = 1000;

                var query = new RequestLogQuery
                {
                    StartTime = startTime ?? DateTime.UtcNow.AddDays(-1),
                    EndTime = endTime ?? DateTime.UtcNow,
                    SlowRequestThresholdMs = thresholdMs,
                    Page = page,
                    PageSize = pageSize,
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
