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
    /// Gateway Route Management Controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/gateway/route")]
    [Route("api/v{version:apiVersion}/gateway/route")]
    [Authorize(Roles = "admin,super_admin")]
    public class GatewayRouteController : ControllerBase
    {
        private readonly IGatewayRouteService _routeService;
        private readonly ILogger<GatewayRouteController> _logger;

        public GatewayRouteController(
            IGatewayRouteService routeService,
            ILogger<GatewayRouteController> logger
        )
        {
            _routeService = routeService;
            _logger = logger;
        }

        /// <summary>
        /// Get paged route list
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] string? routeId = null,
            [FromQuery] string? name = null,
            [FromQuery] string? clusterId = null,
            [FromQuery] string? matchPath = null,
            [FromQuery] bool? isEnabled = null,
            [FromQuery] string? keyword = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                var queryModel = new GatewayRouteQueryModel
                {
                    RouteId = routeId,
                    Name = name,
                    ClusterId = clusterId,
                    MatchPath = matchPath,
                    IsEnabled = isEnabled,
                    Keyword = keyword,
                };

                var result = await _routeService.GetPagedListAsync(queryModel, page, pageSize);

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
                _logger.LogError(ex, "Failed to get route list");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get route list: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Get all enabled routes (for YARP configuration)
        /// </summary>
        [HttpGet("enabled")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEnabled()
        {
            try
            {
                var routes = await _routeService.GetAllEnabledAsync();
                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = routes,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get enabled routes");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get enabled routes: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Get route by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var route = await _routeService.GetAsync(id);
                if (route == null)
                {
                    return NotFound(new { code = 40400, message = "Route not found" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Success",
                        data = route,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get route details");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to get route details: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Create route
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGatewayRouteDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                var route = await _routeService.CreateAsync(dto, username);

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Created successfully",
                        data = route,
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = 40000, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create route");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to create route: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Update route
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGatewayRouteDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { code = 40000, message = "ID mismatch" });
                }

                var username = User.Identity?.Name;
                var route = await _routeService.UpdateAsync(dto, username);

                if (route == null)
                {
                    return NotFound(new { code = 40400, message = "Route not found" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "Updated successfully",
                        data = route,
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = 40000, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update route");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to update route: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Delete route
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _routeService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { code = 40400, message = "Route not found" });
                }

                return Ok(new { code = 20000, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete route");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to delete route: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// Toggle route enabled status
        /// </summary>
        [HttpPatch("{id:guid}/toggle")]
        public async Task<IActionResult> ToggleEnabled(Guid id, [FromBody] ToggleEnabledDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                var result = await _routeService.ToggleEnabledAsync(id, dto.IsEnabled, username);

                if (!result)
                {
                    return NotFound(new { code = 40400, message = "Route not found" });
                }

                return Ok(new { code = 20000, message = dto.IsEnabled ? "Enabled" : "Disabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle route status");
                return StatusCode(
                    500,
                    new { code = 50000, message = "Failed to toggle route status: " + ex.Message }
                );
            }
        }
    }

    /// <summary>
    /// Toggle enabled status DTO
    /// </summary>
    public class ToggleEnabledDto
    {
        public bool IsEnabled { get; set; }
    }
}
