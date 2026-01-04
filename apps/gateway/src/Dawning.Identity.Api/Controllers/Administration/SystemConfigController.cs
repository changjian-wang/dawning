using Dawning.Identity.Api.Helpers;
using Dawning.Identity.Application.Services.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// System configuration controller
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/system-config")]
    [Route("api/v{version:apiVersion}/system-config")]
    [ApiController]
    [Authorize]
    public class SystemConfigController : ControllerBase
    {
        private readonly ISystemConfigService _systemConfigService;
        private readonly AuditLogHelper _auditLogHelper;

        public SystemConfigController(
            ISystemConfigService systemConfigService,
            AuditLogHelper auditLogHelper
        )
        {
            _systemConfigService = systemConfigService;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// Get all configuration groups
        /// </summary>
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _systemConfigService.GetGroupsAsync();
            return Ok(new { success = true, data = groups });
        }

        /// <summary>
        /// Get configurations under a group
        /// </summary>
        [HttpGet("group/{group}")]
        public async Task<IActionResult> GetByGroup(string group)
        {
            var configs = await _systemConfigService.GetByGroupAsync(group);
            return Ok(new { success = true, data = configs });
        }

        /// <summary>
        /// Get configuration value
        /// </summary>
        [HttpGet("{group}/{key}")]
        public async Task<IActionResult> GetValue(string group, string key)
        {
            var value = await _systemConfigService.GetValueAsync(group, key);
            return Ok(new { success = true, data = value });
        }

        /// <summary>
        /// Set configuration value
        /// </summary>
        [HttpPost("{group}/{key}")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> SetValue(
            string group,
            string key,
            [FromBody] SetConfigRequest request
        )
        {
            var result = await _systemConfigService.SetValueAsync(
                group,
                key,
                request.Value,
                request.Description
            );

            if (result)
            {
                await _auditLogHelper.LogAsync(
                    "SetConfig",
                    "SystemConfig",
                    Guid.Empty,
                    $"Set configuration: {group}.{key}",
                    null,
                    new
                    {
                        Group = group,
                        Key = key,
                        Value = request.Value,
                    }
                );
            }

            return Ok(new { success = result });
        }

        /// <summary>
        /// Batch update configurations
        /// </summary>
        [HttpPost("batch")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> BatchUpdate([FromBody] BatchConfigRequest request)
        {
            var result = await _systemConfigService.BatchUpdateAsync(request.Configs);

            if (result)
            {
                var configCount = request.Configs.Count();
                await _auditLogHelper.LogAsync(
                    "BatchUpdateConfig",
                    "SystemConfig",
                    Guid.Empty,
                    $"Batch updated configurations, count: {configCount}",
                    null,
                    new { ConfigCount = configCount }
                );
            }

            return Ok(new { success = result });
        }

        /// <summary>
        /// Delete configuration
        /// </summary>
        [HttpDelete("{group}/{key}")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> Delete(string group, string key)
        {
            var result = await _systemConfigService.DeleteAsync(group, key);

            if (result)
            {
                await _auditLogHelper.LogAsync(
                    "DeleteConfig",
                    "SystemConfig",
                    Guid.Empty,
                    $"Deleted configuration: {group}.{key}"
                );
            }

            return Ok(new { success = result });
        }

        /// <summary>
        /// Get configuration update timestamp (for hot reload detection)
        /// </summary>
        [HttpGet("timestamp")]
        public async Task<IActionResult> GetTimestamp()
        {
            var timestamp = await _systemConfigService.GetLastUpdateTimestampAsync();
            return Ok(new { success = true, data = timestamp });
        }

        /// <summary>
        /// Initialize default configuration
        /// </summary>
        [HttpPost("init-defaults")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> InitDefaults()
        {
            // System configuration
            await _systemConfigService.SetValueAsync(
                "System",
                "AppName",
                "Dawning Gateway",
                "Application name"
            );
            await _systemConfigService.SetValueAsync("System", "AppVersion", "1.0.0", "Application version");
            await _systemConfigService.SetValueAsync(
                "System",
                "DefaultLanguage",
                "zh-CN",
                "Default language"
            );
            await _systemConfigService.SetValueAsync(
                "System",
                "TimeZone",
                "Asia/Shanghai",
                "Default timezone"
            );

            // Security configuration
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordMinLength",
                "8",
                "Minimum password length"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireDigit",
                "true",
                "Password requires digit"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireLowercase",
                "true",
                "Password requires lowercase letter"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireUppercase",
                "true",
                "Password requires uppercase letter"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireNonAlphanumeric",
                "false",
                "Password requires special character"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "LockoutMaxAttempts",
                "5",
                "Maximum attempts before lockout"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "LockoutDurationMinutes",
                "15",
                "Lockout duration (minutes)"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "AccessTokenLifetimeMinutes",
                "60",
                "Access token lifetime (minutes)"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "RefreshTokenLifetimeDays",
                "7",
                "Refresh token lifetime (days)"
            );

            // Email configuration
            await _systemConfigService.SetValueAsync(
                "Email",
                "SmtpHost",
                "smtp.example.com",
                "SMTP server address"
            );
            await _systemConfigService.SetValueAsync("Email", "SmtpPort", "587", "SMTP port");
            await _systemConfigService.SetValueAsync("Email", "SmtpUsername", "", "SMTP username");
            await _systemConfigService.SetValueAsync("Email", "SmtpPassword", "", "SMTP password");
            await _systemConfigService.SetValueAsync("Email", "EnableSsl", "true", "Enable SSL");
            await _systemConfigService.SetValueAsync(
                "Email",
                "FromAddress",
                "noreply@example.com",
                "From address"
            );
            await _systemConfigService.SetValueAsync(
                "Email",
                "FromName",
                "Dawning Gateway",
                "From name"
            );

            // Logging configuration
            await _systemConfigService.SetValueAsync(
                "Logging",
                "LogLevel",
                "Information",
                "Log level"
            );
            await _systemConfigService.SetValueAsync(
                "Logging",
                "RetentionDays",
                "30",
                "Log retention days"
            );
            await _systemConfigService.SetValueAsync(
                "Logging",
                "EnableRequestLogging",
                "true",
                "Enable request logging"
            );
            await _systemConfigService.SetValueAsync(
                "Logging",
                "EnableAuditLogging",
                "true",
                "Enable audit logging"
            );

            // Gateway configuration
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "EnableRateLimiting",
                "true",
                "Enable rate limiting"
            );
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "DefaultRateLimit",
                "100",
                "Default rate limit per minute"
            );
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "EnableCaching",
                "false",
                "Enable caching"
            );
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "CacheExpirationMinutes",
                "5",
                "Cache expiration time (minutes)"
            );

            return Ok(new { success = true, message = "Default configuration initialized" });
        }
    }

    public class SetConfigRequest
    {
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BatchConfigRequest
    {
        public IEnumerable<SystemConfigItemDto> Configs { get; set; } =
            Enumerable.Empty<SystemConfigItemDto>();
    }
}
