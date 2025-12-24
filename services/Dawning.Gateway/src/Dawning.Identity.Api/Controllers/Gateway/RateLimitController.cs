using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Gateway
{
    /// <summary>
    /// 限流策略管理控制器
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/rate-limit")]
    [Route("api/v{version:apiVersion}/rate-limit")]
    [ApiController]
    [Authorize]
    public class RateLimitController : ControllerBase
    {
        private readonly IRateLimitService _rateLimitService;

        public RateLimitController(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
        }

        #region Rate Limit Policies

        /// <summary>
        /// 获取所有限流策略
        /// </summary>
        [HttpGet("policies")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var policies = await _rateLimitService.GetAllPoliciesAsync();
            return Ok(new { success = true, data = policies });
        }

        /// <summary>
        /// 获取限流策略详情
        /// </summary>
        [HttpGet("policies/{id}")]
        public async Task<IActionResult> GetPolicy(Guid id)
        {
            var policy = await _rateLimitService.GetPolicyByIdAsync(id);
            if (policy == null)
                return NotFound(new { success = false, message = "Policy not found" });

            return Ok(new { success = true, data = policy });
        }

        /// <summary>
        /// 创建限流策略
        /// </summary>
        [HttpPost("policies")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> CreatePolicy([FromBody] CreateRateLimitPolicyDto dto)
        {
            // 检查名称是否已存在
            var existing = await _rateLimitService.GetPolicyByNameAsync(dto.Name);
            if (existing != null)
                return BadRequest(new { success = false, message = "Policy name already exists" });

            var id = await _rateLimitService.CreatePolicyAsync(dto);
            return Ok(new { success = true, data = new { id } });
        }

        /// <summary>
        /// 更新限流策略
        /// </summary>
        [HttpPut("policies/{id}")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> UpdatePolicy(
            Guid id,
            [FromBody] UpdateRateLimitPolicyDto dto
        )
        {
            dto.Id = id;
            var result = await _rateLimitService.UpdatePolicyAsync(dto);
            return Ok(new { success = result });
        }

        /// <summary>
        /// 删除限流策略
        /// </summary>
        [HttpDelete("policies/{id}")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            var result = await _rateLimitService.DeletePolicyAsync(id);
            return Ok(new { success = result });
        }

        #endregion

        #region IP Access Rules

        /// <summary>
        /// 获取 IP 访问规则列表（分页）
        /// </summary>
        [HttpGet("ip-rules")]
        public async Task<IActionResult> GetIpRules(
            [FromQuery] string? ruleType,
            [FromQuery] bool? isEnabled,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var (items, total) = await _rateLimitService.GetIpRulesPagedAsync(
                ruleType,
                isEnabled,
                page,
                pageSize
            );

            return Ok(
                new
                {
                    success = true,
                    data = new
                    {
                        items,
                        total,
                        page,
                        pageSize,
                    },
                }
            );
        }

        /// <summary>
        /// 获取 IP 规则详情
        /// </summary>
        [HttpGet("ip-rules/{id}")]
        public async Task<IActionResult> GetIpRule(Guid id)
        {
            var rule = await _rateLimitService.GetIpRuleByIdAsync(id);
            if (rule == null)
                return NotFound(new { success = false, message = "Rule not found" });

            return Ok(new { success = true, data = rule });
        }

        /// <summary>
        /// 获取活跃的黑名单
        /// </summary>
        [HttpGet("ip-rules/blacklist")]
        public async Task<IActionResult> GetBlacklist()
        {
            var rules = await _rateLimitService.GetActiveRulesByTypeAsync("blacklist");
            return Ok(new { success = true, data = rules });
        }

        /// <summary>
        /// 获取活跃的白名单
        /// </summary>
        [HttpGet("ip-rules/whitelist")]
        public async Task<IActionResult> GetWhitelist()
        {
            var rules = await _rateLimitService.GetActiveRulesByTypeAsync("whitelist");
            return Ok(new { success = true, data = rules });
        }

        /// <summary>
        /// 创建 IP 规则
        /// </summary>
        [HttpPost("ip-rules")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> CreateIpRule([FromBody] CreateIpAccessRuleDto dto)
        {
            var createdBy = User.Identity?.Name;
            var id = await _rateLimitService.CreateIpRuleAsync(dto, createdBy);
            return Ok(new { success = true, data = new { id } });
        }

        /// <summary>
        /// 更新 IP 规则
        /// </summary>
        [HttpPut("ip-rules/{id}")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> UpdateIpRule(Guid id, [FromBody] UpdateIpAccessRuleDto dto)
        {
            dto.Id = id;
            var result = await _rateLimitService.UpdateIpRuleAsync(dto);
            return Ok(new { success = result });
        }

        /// <summary>
        /// 删除 IP 规则
        /// </summary>
        [HttpDelete("ip-rules/{id}")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> DeleteIpRule(Guid id)
        {
            var result = await _rateLimitService.DeleteIpRuleAsync(id);
            return Ok(new { success = result });
        }

        /// <summary>
        /// 检查 IP 是否在黑名单中
        /// </summary>
        [HttpGet("check-blacklist")]
        public async Task<IActionResult> CheckBlacklist([FromQuery] string ip)
        {
            var isBlacklisted = await _rateLimitService.IsIpBlacklistedAsync(ip);
            return Ok(new { success = true, data = new { ip, isBlacklisted } });
        }

        /// <summary>
        /// 检查 IP 是否在白名单中
        /// </summary>
        [HttpGet("check-whitelist")]
        public async Task<IActionResult> CheckWhitelist([FromQuery] string ip)
        {
            var isWhitelisted = await _rateLimitService.IsIpWhitelistedAsync(ip);
            return Ok(new { success = true, data = new { ip, isWhitelisted } });
        }

        #endregion
    }
}
