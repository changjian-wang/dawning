using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Controllers
{
    /// <summary>
    /// Health check controller
    /// Provides system health status and monitoring metrics
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        public HealthController(ILogger<HealthController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Basic health check
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(
                new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    uptime = (DateTime.UtcNow - _startTime).ToString(@"dd\.hh\:mm\:ss"),
                }
            );
        }

        /// <summary>
        /// Detailed health check (including database connection)
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            var apiCheck = await CheckApiHealth();
            var dbCheck = await CheckDatabaseHealth();
            var memCheck = CheckMemoryHealth();

            var healthStatus = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                uptime = (DateTime.UtcNow - _startTime).ToString(@"dd\.hh\:mm\:ss"),
                checks = new
                {
                    api = apiCheck,
                    database = dbCheck,
                    memory = memCheck,
                },
            };

            var hasFailures =
                apiCheck.status != "Healthy"
                || dbCheck.status != "Healthy"
                || memCheck.status != "Healthy";

            if (hasFailures)
            {
                return StatusCode(
                    503,
                    new
                    {
                        status = "Unhealthy",
                        timestamp = healthStatus.timestamp,
                        uptime = healthStatus.uptime,
                        checks = healthStatus.checks,
                    }
                );
            }

            return Ok(healthStatus);
        }

        /// <summary>
        /// Readiness check (for Kubernetes readiness probe)
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // Check database connection
                var dbCheck = await CheckDatabaseHealth();
                if (dbCheck.status != "Healthy")
                {
                    return StatusCode(
                        503,
                        new { status = "NotReady", reason = "Database unavailable" }
                    );
                }

                return Ok(new { status = "Ready" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new { status = "NotReady", reason = ex.Message });
            }
        }

        /// <summary>
        /// Liveness check (for Kubernetes liveness probe)
        /// </summary>
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { status = "Alive" });
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        [HttpGet("metrics")]
        public IActionResult Metrics()
        {
            var process = Process.GetCurrentProcess();

            return Ok(
                new
                {
                    timestamp = DateTime.UtcNow,
                    uptime = (DateTime.UtcNow - _startTime).ToString(@"dd\.hh\:mm\:ss"),
                    memory = new
                    {
                        workingSet = FormatBytes(process.WorkingSet64),
                        privateMemory = FormatBytes(process.PrivateMemorySize64),
                        virtualMemory = FormatBytes(process.VirtualMemorySize64),
                        gcMemory = FormatBytes(GC.GetTotalMemory(false)),
                    },
                    cpu = new
                    {
                        totalProcessorTime = process.TotalProcessorTime.ToString(@"hh\:mm\:ss"),
                        userProcessorTime = process.UserProcessorTime.ToString(@"hh\:mm\:ss"),
                    },
                    threads = new { count = process.Threads.Count },
                    gc = new
                    {
                        gen0Collections = GC.CollectionCount(0),
                        gen1Collections = GC.CollectionCount(1),
                        gen2Collections = GC.CollectionCount(2),
                    },
                }
            );
        }

        private async Task<dynamic> CheckApiHealth()
        {
            try
            {
                return new { status = "Healthy", responseTime = "0ms" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API health check failed");
                return new { status = "Unhealthy", error = ex.Message };
            }
        }

        private async Task<dynamic> CheckDatabaseHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Try a simple database query
                var config = await _unitOfWork.SystemConfig.GetPagedListAsync(
                    new Dawning.Identity.Domain.Models.Administration.SystemConfigModel(),
                    1,
                    1
                );
                stopwatch.Stop();

                return new
                {
                    status = "Healthy",
                    responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database health check failed");
                return new
                {
                    status = "Unhealthy",
                    error = ex.Message,
                    responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                };
            }
        }

        private dynamic CheckMemoryHealth()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var workingSetMb = process.WorkingSet64 / 1024 / 1024;
                var threshold = 1024; // 1GB

                return new
                {
                    status = workingSetMb < threshold ? "Healthy" : "Warning",
                    workingSet = FormatBytes(process.WorkingSet64),
                    threshold = $"{threshold}MB",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory health check failed");
                return new { status = "Unhealthy", error = ex.Message };
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
