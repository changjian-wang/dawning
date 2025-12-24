using System;
using System.Threading.Tasks;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.MultiTenancy
{
    /// <summary>
    /// 租户管理控制器
    /// 仅限超级管理员访问
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "super_admin")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<TenantController> _logger;

        public TenantController(
            ITenantService tenantService,
            ITenantContext tenantContext,
            ILogger<TenantController> logger
        )
        {
            _tenantService = tenantService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前租户信息
        /// </summary>
        [HttpGet("current")]
        [AllowAnonymous]
        public IActionResult GetCurrentTenant()
        {
            return Ok(
                new
                {
                    code = 20000,
                    data = new
                    {
                        tenantId = _tenantContext.TenantId,
                        tenantName = _tenantContext.TenantName,
                        isHost = _tenantContext.IsHost,
                    },
                }
            );
        }

        /// <summary>
        /// 分页获取租户列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? keyword = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                var result = await _tenantService.GetPagedAsync(keyword, isActive, page, pageSize);

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
                _logger.LogError(ex, "获取租户列表失败");
                return StatusCode(500, ApiResponse.Error(50000, "获取租户列表失败"));
            }
        }

        /// <summary>
        /// 获取所有租户
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tenants = await _tenantService.GetAllAsync();
                return Ok(new { code = 20000, data = tenants });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有租户失败");
                return StatusCode(500, new { code = 50000, message = "获取所有租户失败" });
            }
        }

        /// <summary>
        /// 获取所有启用的租户
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var tenants = await _tenantService.GetActiveTenantsAsync();
                return Ok(new { code = 20000, data = tenants });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取启用租户失败");
                return StatusCode(500, new { code = 50000, message = "获取启用租户失败" });
            }
        }

        /// <summary>
        /// 根据ID获取租户
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var tenant = await _tenantService.GetAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { code = 40400, message = "租户不存在" });
                }
                return Ok(new { code = 20000, data = tenant });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取租户失败: {TenantId}", id);
                return StatusCode(500, new { code = 50000, message = "获取租户失败" });
            }
        }

        /// <summary>
        /// 创建租户
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
        {
            try
            {
                var tenant = new Tenant
                {
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    Domain = request.Domain,
                    Email = request.Email,
                    Phone = request.Phone,
                    LogoUrl = request.LogoUrl,
                    Plan = request.Plan ?? "free",
                    MaxUsers = request.MaxUsers,
                    MaxStorageMB = request.MaxStorageMB,
                    SubscriptionExpiresAt = request.SubscriptionExpiresAt,
                    IsActive = request.IsActive ?? true,
                };

                var created = await _tenantService.CreateAsync(tenant);
                _logger.LogInformation(
                    "创建租户: {TenantCode} ({TenantId})",
                    created.Code,
                    created.Id
                );

                return Ok(
                    new
                    {
                        code = 20000,
                        data = created,
                        message = "租户创建成功",
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = 40000, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建租户失败");
                return StatusCode(500, new { code = 50000, message = "创建租户失败" });
            }
        }

        /// <summary>
        /// 更新租户
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request)
        {
            try
            {
                var existing = await _tenantService.GetAsync(id);
                if (existing == null)
                {
                    return NotFound(new { code = 40400, message = "租户不存在" });
                }

                existing.Code = request.Code ?? existing.Code;
                existing.Name = request.Name ?? existing.Name;
                existing.Description = request.Description;
                existing.Domain = request.Domain;
                existing.Email = request.Email;
                existing.Phone = request.Phone;
                existing.LogoUrl = request.LogoUrl;
                existing.Plan = request.Plan ?? existing.Plan;
                existing.MaxUsers = request.MaxUsers;
                existing.MaxStorageMB = request.MaxStorageMB;
                existing.SubscriptionExpiresAt = request.SubscriptionExpiresAt;
                existing.Settings = request.Settings;

                if (request.IsActive.HasValue)
                {
                    existing.IsActive = request.IsActive.Value;
                }

                var updated = await _tenantService.UpdateAsync(existing);
                _logger.LogInformation(
                    "更新租户: {TenantCode} ({TenantId})",
                    updated.Code,
                    updated.Id
                );

                return Ok(
                    new
                    {
                        code = 20000,
                        data = updated,
                        message = "租户更新成功",
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = 40000, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新租户失败: {TenantId}", id);
                return StatusCode(500, new { code = 50000, message = "更新租户失败" });
            }
        }

        /// <summary>
        /// 删除租户
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _tenantService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { code = 40400, message = "租户不存在" });
                }

                _logger.LogInformation("删除租户: {TenantId}", id);
                return Ok(new { code = 20000, message = "租户删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除租户失败: {TenantId}", id);
                return StatusCode(500, new { code = 50000, message = "删除租户失败" });
            }
        }

        /// <summary>
        /// 设置租户启用状态
        /// </summary>
        [HttpPut("{id:guid}/active")]
        public async Task<IActionResult> SetActive(Guid id, [FromBody] SetActiveRequest request)
        {
            try
            {
                var result = await _tenantService.SetActiveAsync(id, request.IsActive);
                if (!result)
                {
                    return NotFound(new { code = 40400, message = "租户不存在" });
                }

                var status = request.IsActive ? "启用" : "禁用";
                _logger.LogInformation("设置租户状态: {TenantId} -> {Status}", id, status);
                return Ok(new { code = 20000, message = $"租户已{status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置租户状态失败: {TenantId}", id);
                return StatusCode(500, new { code = 50000, message = "设置租户状态失败" });
            }
        }

        /// <summary>
        /// 检查租户代码是否可用
        /// </summary>
        [HttpGet("check-code")]
        public async Task<IActionResult> CheckCode(
            [FromQuery] string code,
            [FromQuery] Guid? excludeId = null
        )
        {
            var isAvailable = await _tenantService.IsCodeAvailableAsync(code, excludeId);
            return Ok(new { code = 20000, data = new { available = isAvailable } });
        }

        /// <summary>
        /// 检查域名是否可用
        /// </summary>
        [HttpGet("check-domain")]
        public async Task<IActionResult> CheckDomain(
            [FromQuery] string domain,
            [FromQuery] Guid? excludeId = null
        )
        {
            var isAvailable = await _tenantService.IsDomainAvailableAsync(domain, excludeId);
            return Ok(new { code = 20000, data = new { available = isAvailable } });
        }
    }

    #region Request DTOs

    public class CreateTenantRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Domain { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? LogoUrl { get; set; }
        public string? Plan { get; set; }
        public int? MaxUsers { get; set; }
        public int? MaxStorageMB { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateTenantRequest
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Domain { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? LogoUrl { get; set; }
        public string? Plan { get; set; }
        public int? MaxUsers { get; set; }
        public int? MaxStorageMB { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public string? Settings { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SetActiveRequest
    {
        public bool IsActive { get; set; }
    }

    #endregion
}
