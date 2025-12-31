using Dawning.Identity.Api.Helpers;
using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration;

/// <summary>
/// Alert Management Controller
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
        AuditLogHelper auditLogHelper
    )
    {
        _alertService = alertService;
        _logger = logger;
        _auditLogHelper = auditLogHelper;
    }

    #region Alert Rule Management

    /// <summary>
    /// Get all alert rules
    /// </summary>
    [HttpGet("rules")]
    public async Task<ActionResult<IEnumerable<AlertRuleDto>>> GetAllRules()
    {
        var rules = await _alertService.GetAllRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// Get enabled alert rules
    /// </summary>
    [HttpGet("rules/enabled")]
    public async Task<ActionResult<IEnumerable<AlertRuleDto>>> GetEnabledRules()
    {
        var rules = await _alertService.GetEnabledRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// Get specific alert rule
    /// </summary>
    [HttpGet("rules/{id:long}")]
    public async Task<ActionResult<AlertRuleDto>> GetRule(long id)
    {
        var rule = await _alertService.GetRuleByIdAsync(id);
        if (rule == null)
        {
            return NotFound(new { message = "Alert rule not found" });
        }
        return Ok(rule);
    }

    /// <summary>
    /// Create alert rule
    /// </summary>
    [HttpPost("rules")]
    public async Task<ActionResult<AlertRuleDto>> CreateRule(
        [FromBody] CreateAlertRuleRequest request
    )
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
            $"Created alert rule: {rule.Name}",
            null,
            request
        );

        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Update alert rule
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
            return NotFound(new { message = "Alert rule not found" });
        }

        _logger.LogInformation("Updated alert rule: {RuleName}", rule.Name);

        await _auditLogHelper.LogAsync(
            "UpdateAlertRule",
            "AlertRule",
            null,
            $"Updated alert rule: {rule.Name}",
            null,
            request
        );

        return Ok(rule);
    }

    /// <summary>
    /// Delete alert rule
    /// </summary>
    [HttpDelete("rules/{id:long}")]
    public async Task<IActionResult> DeleteRule(long id)
    {
        var success = await _alertService.DeleteRuleAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Alert rule not found" });
        }

        _logger.LogInformation("Deleted alert rule: {Id}", id);

        await _auditLogHelper.LogAsync(
            "DeleteAlertRule",
            "AlertRule",
            null,
            $"Deleted alert rule ID: {id}"
        );

        return NoContent();
    }

    /// <summary>
    /// Enable/disable alert rule
    /// </summary>
    [HttpPatch("rules/{id:long}/enabled")]
    public async Task<IActionResult> SetRuleEnabled(
        long id,
        [FromBody] SetRuleEnabledRequest request
    )
    {
        var success = await _alertService.SetRuleEnabledAsync(id, request.IsEnabled);
        if (!success)
        {
            return NotFound(new { message = "Alert rule not found" });
        }

        _logger.LogInformation("Set alert rule {Id} enabled: {IsEnabled}", id, request.IsEnabled);

        await _auditLogHelper.LogAsync(
            request.IsEnabled ? "EnableAlertRule" : "DisableAlertRule",
            "AlertRule",
            null,
            $"{(request.IsEnabled ? "Enabled" : "Disabled")} alert rule ID: {id}"
        );

        return Ok(new { message = request.IsEnabled ? "Rule enabled" : "Rule disabled" });
    }

    #endregion

    #region Alert History Management

    /// <summary>
    /// Query alert history (paged)
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult> GetAlertHistory([FromQuery] AlertHistoryQueryParams queryParams)
    {
        var result = await _alertService.GetAlertHistoryAsync(queryParams);
        var totalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize);
        return Ok(
            new
            {
                items = result.Items,
                total = result.TotalCount,
                page = result.PageIndex,
                pageSize = result.PageSize,
                totalPages,
            }
        );
    }

    /// <summary>
    /// Get specific alert history
    /// </summary>
    [HttpGet("history/{id:long}")]
    public async Task<ActionResult<AlertHistoryDto>> GetAlertHistoryById(long id)
    {
        var history = await _alertService.GetAlertHistoryByIdAsync(id);
        if (history == null)
        {
            return NotFound(new { message = "Alert record not found" });
        }
        return Ok(history);
    }

    /// <summary>
    /// Get unresolved alerts
    /// </summary>
    [HttpGet("unresolved")]
    public async Task<ActionResult<IEnumerable<AlertHistoryDto>>> GetUnresolvedAlerts()
    {
        var alerts = await _alertService.GetUnresolvedAlertsAsync();
        return Ok(alerts);
    }

    /// <summary>
    /// Update alert status (acknowledge/resolve)
    /// </summary>
    [HttpPatch("history/{id:long}/status")]
    public async Task<IActionResult> UpdateAlertStatus(
        long id,
        [FromBody] UpdateAlertStatusRequest request
    )
    {
        // Auto-fill operator
        if (string.IsNullOrEmpty(request.ResolvedBy))
        {
            request.ResolvedBy = User.Identity?.Name ?? "system";
        }

        var success = await _alertService.UpdateAlertStatusAsync(id, request);
        if (!success)
        {
            return NotFound(new { message = "Alert record not found" });
        }

        _logger.LogInformation(
            "Updated alert {Id} status to {Status} by {User}",
            id,
            request.Status,
            request.ResolvedBy
        );
        return Ok(new { message = "Status updated successfully" });
    }

    /// <summary>
    /// Batch acknowledge alerts
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

        return Ok(
            new
            {
                message = $"Successfully acknowledged {successCount}/{request.Ids.Count} alerts",
                successCount,
                totalCount = request.Ids.Count,
            }
        );
    }

    /// <summary>
    /// Batch resolve alerts
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

        return Ok(
            new
            {
                message = $"Successfully resolved {successCount}/{request.Ids.Count} alerts",
                successCount,
                totalCount = request.Ids.Count,
            }
        );
    }

    #endregion

    #region Alert Check and Statistics

    /// <summary>
    /// Manually trigger alert check
    /// </summary>
    [HttpPost("check")]
    public async Task<ActionResult<AlertCheckResult>> TriggerAlertCheck()
    {
        _logger.LogInformation("Manual alert check triggered by {User}", User.Identity?.Name);
        var result = await _alertService.TriggerAlertCheckAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get alert statistics
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
/// Set rule enabled status request
/// </summary>
public class SetRuleEnabledRequest
{
    public bool IsEnabled { get; set; }
}

/// <summary>
/// Batch alert action request
/// </summary>
public class BatchAlertActionRequest
{
    public List<long> Ids { get; set; } = new();
}
