using Dawning.Identity.Api.Helpers;
using Dawning.Identity.Application.Services.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// 系统配置控制器
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

        public SystemConfigController(ISystemConfigService systemConfigService, AuditLogHelper auditLogHelper)
        {
            _systemConfigService = systemConfigService;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// 获取所有配置分组
        /// </summary>
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _systemConfigService.GetGroupsAsync();
            return Ok(new { success = true, data = groups });
        }

        /// <summary>
        /// 获取分组下的配置
        /// </summary>
        [HttpGet("group/{group}")]
        public async Task<IActionResult> GetByGroup(string group)
        {
            var configs = await _systemConfigService.GetByGroupAsync(group);
            return Ok(new { success = true, data = configs });
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        [HttpGet("{group}/{key}")]
        public async Task<IActionResult> GetValue(string group, string key)
        {
            var value = await _systemConfigService.GetValueAsync(group, key);
            return Ok(new { success = true, data = value });
        }

        /// <summary>
        /// 设置配置值
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
                    $"设置配置: {group}.{key}",
                    null,
                    new { Group = group, Key = key, Value = request.Value }
                );
            }
            
            return Ok(new { success = result });
        }

        /// <summary>
        /// 批量更新配置
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
                    $"批量更新配置，数量: {configCount}",
                    null,
                    new { ConfigCount = configCount }
                );
            }
            
            return Ok(new { success = result });
        }

        /// <summary>
        /// 删除配置
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
                    $"删除配置: {group}.{key}"
                );
            }
            
            return Ok(new { success = result });
        }

        /// <summary>
        /// 获取配置更新时间戳（用于热更新检测）
        /// </summary>
        [HttpGet("timestamp")]
        public async Task<IActionResult> GetTimestamp()
        {
            var timestamp = await _systemConfigService.GetLastUpdateTimestampAsync();
            return Ok(new { success = true, data = timestamp });
        }

        /// <summary>
        /// 初始化默认配置
        /// </summary>
        [HttpPost("init-defaults")]
        [Authorize(Policy = "SystemAdmin")]
        public async Task<IActionResult> InitDefaults()
        {
            // 系统配置
            await _systemConfigService.SetValueAsync(
                "System",
                "AppName",
                "Dawning Gateway",
                "应用名称"
            );
            await _systemConfigService.SetValueAsync("System", "AppVersion", "1.0.0", "应用版本");
            await _systemConfigService.SetValueAsync(
                "System",
                "DefaultLanguage",
                "zh-CN",
                "默认语言"
            );
            await _systemConfigService.SetValueAsync(
                "System",
                "TimeZone",
                "Asia/Shanghai",
                "默认时区"
            );

            // 安全配置
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordMinLength",
                "8",
                "密码最小长度"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireDigit",
                "true",
                "密码需要数字"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireLowercase",
                "true",
                "密码需要小写字母"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireUppercase",
                "true",
                "密码需要大写字母"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "PasswordRequireNonAlphanumeric",
                "false",
                "密码需要特殊字符"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "LockoutMaxAttempts",
                "5",
                "锁定前最大尝试次数"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "LockoutDurationMinutes",
                "15",
                "锁定持续时间（分钟）"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "AccessTokenLifetimeMinutes",
                "60",
                "访问令牌有效期（分钟）"
            );
            await _systemConfigService.SetValueAsync(
                "Security",
                "RefreshTokenLifetimeDays",
                "7",
                "刷新令牌有效期（天）"
            );

            // 邮件配置
            await _systemConfigService.SetValueAsync(
                "Email",
                "SmtpHost",
                "smtp.example.com",
                "SMTP服务器地址"
            );
            await _systemConfigService.SetValueAsync("Email", "SmtpPort", "587", "SMTP端口");
            await _systemConfigService.SetValueAsync("Email", "SmtpUsername", "", "SMTP用户名");
            await _systemConfigService.SetValueAsync("Email", "SmtpPassword", "", "SMTP密码");
            await _systemConfigService.SetValueAsync("Email", "EnableSsl", "true", "启用SSL");
            await _systemConfigService.SetValueAsync(
                "Email",
                "FromAddress",
                "noreply@example.com",
                "发件人地址"
            );
            await _systemConfigService.SetValueAsync(
                "Email",
                "FromName",
                "Dawning Gateway",
                "发件人名称"
            );

            // 日志配置
            await _systemConfigService.SetValueAsync(
                "Logging",
                "LogLevel",
                "Information",
                "日志级别"
            );
            await _systemConfigService.SetValueAsync(
                "Logging",
                "RetentionDays",
                "30",
                "日志保留天数"
            );
            await _systemConfigService.SetValueAsync(
                "Logging",
                "EnableRequestLogging",
                "true",
                "启用请求日志"
            );
            await _systemConfigService.SetValueAsync(
                "Logging",
                "EnableAuditLogging",
                "true",
                "启用审计日志"
            );

            // 网关配置
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "EnableRateLimiting",
                "true",
                "启用限流"
            );
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "DefaultRateLimit",
                "100",
                "默认限流次数/分钟"
            );
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "EnableCaching",
                "false",
                "启用缓存"
            );
            await _systemConfigService.SetValueAsync(
                "Gateway",
                "CacheExpirationMinutes",
                "5",
                "缓存过期时间（分钟）"
            );

            return Ok(new { success = true, message = "默认配置已初始化" });
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
