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
    /// 网关路由管理控制器
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
        /// 分页获取路由列表
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
                _logger.LogError(ex, "获取路由列表失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取路由列表失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 获取所有启用的路由（用于YARP配置）
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
                _logger.LogError(ex, "获取启用的路由失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取启用的路由失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 根据ID获取路由
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var route = await _routeService.GetAsync(id);
                if (route == null)
                {
                    return NotFound(new { code = 40400, message = "路由不存在" });
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
                _logger.LogError(ex, "获取路由详情失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取路由详情失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 创建路由
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
                        message = "创建成功",
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
                _logger.LogError(ex, "创建路由失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "创建路由失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 更新路由
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGatewayRouteDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { code = 40000, message = "ID不匹配" });
                }

                var username = User.Identity?.Name;
                var route = await _routeService.UpdateAsync(dto, username);

                if (route == null)
                {
                    return NotFound(new { code = 40400, message = "路由不存在" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "更新成功",
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
                _logger.LogError(ex, "更新路由失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "更新路由失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 删除路由
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _routeService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { code = 40400, message = "路由不存在" });
                }

                return Ok(new { code = 20000, message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除路由失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "删除路由失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 切换路由启用状态
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
                    return NotFound(new { code = 40400, message = "路由不存在" });
                }

                return Ok(new { code = 20000, message = dto.IsEnabled ? "已启用" : "已禁用" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换路由状态失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "切换路由状态失败: " + ex.Message }
                );
            }
        }
    }

    /// <summary>
    /// 切换启用状态 DTO
    /// </summary>
    public class ToggleEnabledDto
    {
        public bool IsEnabled { get; set; }
    }
}
