using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Dawning.Identity.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration;

/// <summary>
/// 告警管理控制器
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/admin/[controller]")]
[Route("api/v{version:apiVersion}/admin/[controller]")]
[Authorize]
public class AlertController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertController> _logger;
    private readonly AuditLogHelper _auditLogHelper;

    public AlertController(
        IAlertService alertService, 
        ILogger<AlertController> logger,
        AuditLogHelper auditLogHelper)
    {
        _alertService = alertService;
        _logger = logger;
        _auditLogHelper = auditLogHelper;
    }

    #region 告警规则管理

    /// <summary>
    /// 获取所有告警规则
    /// </summary>
    [HttpGet("rules")]
    public async Task<ActionResult<IEnumerable<AlertRuleDto>>> GetAllRules()
    {
        var rules = await _alertService.GetAllRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// 获取已启用的告警规则
    /// </summary>
    [HttpGet("rules/enabled")]
    public async Task<ActionResult<IEnumerable<AlertRuleDto>>> GetEnabledRules()
    {
        var rules = await _alertService.GetEnabledRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// 获取指定告警规则
    /// </summary>
    [HttpGet("rules/{id:long}")]
    public async Task<ActionResult<AlertRuleDto>> GetRule(long id)
    {
        var rule = await _alertService.GetRuleByIdAsync(id);
        if (rule == null)
        {
            return NotFound(new { message = "告警规则不存在" });
        }
        return Ok(rule);
    }

    /// <summary>
    /// 创建告警规则
    /// </summary>
    [HttpPost("rules")]
    public async Task<ActionResult<AlertRuleDto>> CreateRule([FromBody] CreateAlertRuleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var rule = await _alertService.CreateRuleAsync(request);
        _logger.LogInformation("Created alert rule: {RuleName}", rule.Name);
        
        await _auditLogHelper.LogAsync(
            "CreateAlertRule",
            "AlertRule",
            null,
            $"创建告警规则: {rule.Name}",
            null,
            request
        );
        
        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// 更新告警规则
    /// </summary>
    [HttpPut("rules/{id:long}")]
    public async Task<ActionResult<AlertRuleDto>> UpdateRule(
        long id,
        [FromBody] UpdateAlertRuleRequest request
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var rule = await _alertService.UpdateRuleAsync(id, request);
        if (rule == null)
        {
            return NotFound(new { message = "告警规则不存在" });
        }

        _logger.LogInformation("Updated alert rule: {RuleName}", rule.Name);
        
        await _auditLogHelper.LogAsync(
            "UpdateAlertRule",
            "AlertRule",
            null,
            $"更新告警规则: {rule.Name}",
            null,
            request
        );
        
        return Ok(rule);
    }

    /// <summary>
    /// 删除告警规则
    /// </summary>
    [HttpDelete("rules/{id:long}")]
    public async Task<IActionResult> DeleteRule(long id)
    {
        var success = await _alertService.DeleteRuleAsync(id);
        if (!success)
        {
            return NotFound(new { message = "告警规则不存在" });
        }

        _logger.LogInformation("Deleted alert rule: {Id}", id);
        
        await _auditLogHelper.LogAsync(
            "DeleteAlertRule",
            "AlertRule",
            null,
            $"删除告警规则 ID: {id}"
        );
        
        return NoContent();
    }

    /// <summary>
    /// 启用/禁用告警规则
    /// </summary>
    [HttpPatch("rules/{id:long}/enabled")]
    public async Task<IActionResult> SetRuleEnabled(long id, [FromBody] SetRuleEnabledRequest request)
    {
        var success = await _alertService.SetRuleEnabledAsync(id, request.IsEnabled);
        if (!success)
        {
            return NotFound(new { message = "告警规则不存在" });
        }

        _logger.LogInformation(
            "Set alert rule {Id} enabled: {IsEnabled}",
            id,
            request.IsEnabled
        );
        
        await _auditLogHelper.LogAsync(
            request.IsEnabled ? "EnableAlertRule" : "DisableAlertRule",
            "AlertRule",
            null,
            $"{(request.IsEnabled ? "启用" : "禁用")}告警规则 ID: {id}"
        );
        
        return Ok(new { message = request.IsEnabled ? "规则已启用" : "规则已禁用" });
    }

    #endregion

    #region 告警历史管理

    /// <summary>
    /// 查询告警历史 (分页)
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult> GetAlertHistory([FromQuery] AlertHistoryQueryParams queryParams)
    {
        var result = await _alertService.GetAlertHistoryAsync(queryParams);
        var totalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize);
        return Ok(new
        {
            items = result.Items,
            total = result.TotalCount,
            page = result.PageIndex,
            pageSize = result.PageSize,
            totalPages,
        });
    }

    /// <summary>
    /// 获取指定告警历史
    /// </summary>
    [HttpGet("history/{id:long}")]
    public async Task<ActionResult<AlertHistoryDto>> GetAlertHistoryById(long id)
    {
        var history = await _alertService.GetAlertHistoryByIdAsync(id);
        if (history == null)
        {
            return NotFound(new { message = "告警记录不存在" });
        }
        return Ok(history);
    }

    /// <summary>
    /// 获取未解决的告警
    /// </summary>
    [HttpGet("unresolved")]
    public async Task<ActionResult<IEnumerable<AlertHistoryDto>>> GetUnresolvedAlerts()
    {
        var alerts = await _alertService.GetUnresolvedAlertsAsync();
        return Ok(alerts);
    }

    /// <summary>
    /// 更新告警状态 (确认/解决)
    /// </summary>
    [HttpPatch("history/{id:long}/status")]
    public async Task<IActionResult> UpdateAlertStatus(
        long id,
        [FromBody] UpdateAlertStatusRequest request
    )
    {
        // 自动填充操作人
        if (string.IsNullOrEmpty(request.ResolvedBy))
        {
            request.ResolvedBy = User.Identity?.Name ?? "system";
        }

        var success = await _alertService.UpdateAlertStatusAsync(id, request);
        if (!success)
        {
            return NotFound(new { message = "告警记录不存在" });
        }

        _logger.LogInformation(
            "Updated alert {Id} status to {Status} by {User}",
            id,
            request.Status,
            request.ResolvedBy
        );
        return Ok(new { message = "状态更新成功" });
    }

    /// <summary>
    /// 批量确认告警
    /// </summary>
    [HttpPost("history/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlerts([FromBody] BatchAlertActionRequest request)
    {
        var user = User.Identity?.Name ?? "system";
        var successCount = 0;

        foreach (var id in request.Ids)
        {
            var success = await _alertService.UpdateAlertStatusAsync(
                id,
                new UpdateAlertStatusRequest { Status = "acknowledged", ResolvedBy = user }
            );
            if (success)
                successCount++;
        }

        return Ok(new
        {
            message = $"成功确认 {successCount}/{request.Ids.Count} 条告警",
            successCount,
            totalCount = request.Ids.Count,
        });
    }

    /// <summary>
    /// 批量解决告警
    /// </summary>
    [HttpPost("history/resolve")]
    public async Task<IActionResult> ResolveAlerts([FromBody] BatchAlertActionRequest request)
    {
        var user = User.Identity?.Name ?? "system";
        var successCount = 0;

        foreach (var id in request.Ids)
        {
            var success = await _alertService.UpdateAlertStatusAsync(
                id,
                new UpdateAlertStatusRequest { Status = "resolved", ResolvedBy = user }
            );
            if (success)
                successCount++;
        }

        return Ok(new
        {
            message = $"成功解决 {successCount}/{request.Ids.Count} 条告警",
            successCount,
            totalCount = request.Ids.Count,
        });
    }

    #endregion

    #region 告警检查与统计

    /// <summary>
    /// 手动触发告警检查
    /// </summary>
    [HttpPost("check")]
    public async Task<ActionResult<AlertCheckResult>> TriggerAlertCheck()
    {
        _logger.LogInformation("Manual alert check triggered by {User}", User.Identity?.Name);
        var result = await _alertService.TriggerAlertCheckAsync();
        return Ok(result);
    }

    /// <summary>
    /// 获取告警统计
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AlertStatisticsDto>> GetStatistics()
    {
        var stats = await _alertService.GetAlertStatisticsAsync();
        return Ok(stats);
    }

    #endregion
}

/// <summary>
/// 设置规则启用状态请求
/// </summary>
public class SetRuleEnabledRequest
{
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 批量告警操作请求
/// </summary>
public class BatchAlertActionRequest
{
    public List<long> Ids { get; set; } = new();
}
