using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Application.Interfaces.Gateway;
using Dawning.Identity.Domain.Models.Gateway;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Gateway
{
    /// <summary>
    /// Gateway Cluster Management Controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/gateway/cluster")]
    [Route("api/v{version:apiVersion}/gateway/cluster")]
    [Authorize(Roles = "admin,super_admin")]
    public class GatewayClusterController : ControllerBase
    {
        private readonly IGatewayClusterService _clusterService;
        private readonly ILogger<GatewayClusterController> _logger;

        public GatewayClusterController(
            IGatewayClusterService clusterService,
            ILogger<GatewayClusterController> logger
        )
        {
            _clusterService = clusterService;
            _logger = logger;
        }

        /// <summary>
        /// Get paged cluster list
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] string? clusterId = null,
            [FromQuery] string? name = null,
            [FromQuery] string? loadBalancingPolicy = null,
            [FromQuery] bool? isEnabled = null,
            [FromQuery] string? keyword = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                var queryModel = new GatewayClusterQueryModel
                {
                    ClusterId = clusterId,
                    Name = name,
                    LoadBalancingPolicy = loadBalancingPolicy,
                    IsEnabled = isEnabled,
                    Keyword = keyword,
                };

                var result = await _clusterService.GetPagedListAsync(queryModel, page, pageSize);

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
                _logger.LogError(ex, "Failed to get cluster list");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get cluster list: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Get all enabled clusters (for YARP configuration)
        /// </summary>
        [HttpGet("enabled")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEnabled()
        {
            try
            {
                var clusters = await _clusterService.GetAllEnabledAsync();
                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = clusters,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get enabled clusters");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get enabled clusters: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Get cluster options (for dropdown selection)
        /// </summary>
        [HttpGet("options")]
        public async Task<IActionResult> GetOptions()
        {
            try
            {
                var options = await _clusterService.GetOptionsAsync();
                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = options,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cluster options");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get cluster options: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Get cluster by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var cluster = await _clusterService.GetAsync(id);
                if (cluster == null)
                {
                    return NotFound(new { code = 40400, message = "Cluster not found" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = cluster,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cluster details");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get cluster details: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Create cluster
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGatewayClusterDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                var cluster = await _clusterService.CreateAsync(dto, username);

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Created successfully",
                        data = cluster,
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = 40000, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create cluster");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to create cluster: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Update cluster
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGatewayClusterDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { code = 40000, message = "ID mismatch" });
                }

                var username = User.Identity?.Name;
                var cluster = await _clusterService.UpdateAsync(dto, username);

                if (cluster == null)
                {
                    return NotFound(new { code = 40400, message = "Cluster not found" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Updated successfully",
                        data = cluster,
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = 40000, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update cluster");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to update cluster: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Delete cluster
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var (success, errorMessage) = await _clusterService.DeleteAsync(id);

                if (!success)
                {
                    if (errorMessage?.Contains("not found") == true || errorMessage?.Contains("不存在") == true)
                    {
                        return NotFound(new { code = 40400, message = errorMessage });
                    }

                    return BadRequest(new { code = 40000, message = errorMessage });
                }

                return Ok(new { code = 20000, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete cluster");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to delete cluster: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Toggle cluster enabled status
        /// </summary>
        [HttpPatch("{id:guid}/toggle")]
        public async Task<IActionResult> ToggleEnabled(Guid id, [FromBody] ToggleEnabledDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                var result = await _clusterService.ToggleEnabledAsync(id, dto.IsEnabled, username);

                if (!result)
                {
                    return NotFound(new { code = 40400, message = "Cluster not found" });
                }

                return Ok(new { code = 20000, message = dto.IsEnabled ? "Enabled" : "Disabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle cluster status");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to toggle cluster status: " + ex.Message }
                );
            }
        }
    }
}
