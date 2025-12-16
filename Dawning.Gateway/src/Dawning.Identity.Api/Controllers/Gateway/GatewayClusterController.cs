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
    /// 网关集群管理控制器
    /// </summary>
    [ApiController]
    [Route("api/gateway/cluster")]
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
        /// 分页获取集群列表
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
                _logger.LogError(ex, "获取集群列表失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取集群列表失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 获取所有启用的集群（用于YARP配置）
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
                _logger.LogError(ex, "获取启用的集群失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取启用的集群失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 获取集群选项（用于下拉选择）
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
                _logger.LogError(ex, "获取集群选项失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取集群选项失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 根据ID获取集群
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var cluster = await _clusterService.GetAsync(id);
                if (cluster == null)
                {
                    return NotFound(new { code = 40400, message = "集群不存在" });
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
                _logger.LogError(ex, "获取集群详情失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "获取集群详情失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 创建集群
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
                        message = "创建成功",
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
                _logger.LogError(ex, "创建集群失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "创建集群失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 更新集群
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGatewayClusterDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { code = 40000, message = "ID不匹配" });
                }

                var username = User.Identity?.Name;
                var cluster = await _clusterService.UpdateAsync(dto, username);

                if (cluster == null)
                {
                    return NotFound(new { code = 40400, message = "集群不存在" });
                }

                return Ok(
                    new
                    {
                        code = 20000,
                        message = "更新成功",
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
                _logger.LogError(ex, "更新集群失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "更新集群失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 删除集群
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var (success, errorMessage) = await _clusterService.DeleteAsync(id);

                if (!success)
                {
                    if (errorMessage?.Contains("不存在") == true)
                    {
                        return NotFound(new { code = 40400, message = errorMessage });
                    }

                    return BadRequest(new { code = 40000, message = errorMessage });
                }

                return Ok(new { code = 20000, message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除集群失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "删除集群失败: " + ex.Message }
                );
            }
        }

        /// <summary>
        /// 切换集群启用状态
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
                    return NotFound(new { code = 40400, message = "集群不存在" });
                }

                return Ok(new { code = 20000, message = dto.IsEnabled ? "已启用" : "已禁用" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换集群状态失败");
                return StatusCode(
                    500,
                    new { code = 50000, message = "切换集群状态失败: " + ex.Message }
                );
            }
        }
    }
}
