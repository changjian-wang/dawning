using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity.Api.Helpers
{
    /// <summary>
    /// 审计日志辅助类
    /// </summary>
    public class AuditLogHelper
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogHelper(
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 记录审计日志
        /// </summary>
        public async Task LogAsync(
            string action,
            string? entityType = null,
            Guid? entityId = null,
            string? description = null,
            object? oldValues = null,
            object? newValues = null,
            int? statusCode = null
        )
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return;

                var user = httpContext.User;
                var userId = GetUserId(user);
                var username = GetUsername(user);

                var dto = new CreateAuditLogDto
                {
                    UserId = userId,
                    Username = username,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = description,
                    IpAddress = GetIpAddress(httpContext),
                    UserAgent = GetUserAgent(httpContext),
                    RequestPath = httpContext.Request.Path,
                    RequestMethod = httpContext.Request.Method,
                    StatusCode = statusCode ?? httpContext.Response.StatusCode,
                    OldValues = oldValues,
                    NewValues = newValues,
                };

                await _auditLogService.CreateAsync(dto);
            }
            catch (Exception ex)
            {
                // 记录日志但不抛出异常，避免影响主流程
                Console.WriteLine($"Failed to log audit: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        private static Guid? GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
            if (claim != null && Guid.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// 获取用户名
        /// </summary>
        private static string? GetUsername(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("preferred_username")?.Value
                ?? user.Identity?.Name;
        }

        /// <summary>
        /// 获取IP地址
        /// </summary>
        private static string? GetIpAddress(HttpContext context)
        {
            // 优先获取X-Forwarded-For（用于反向代理场景）
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // 获取X-Real-IP
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // 最后获取RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// 获取用户代理
        /// </summary>
        private static string? GetUserAgent(HttpContext context)
        {
            return context.Request.Headers["User-Agent"].FirstOrDefault();
        }
    }
}
