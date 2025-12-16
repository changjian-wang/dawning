using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Dawning.Identity.Api.Controllers
{
    /// <summary>
    /// Token 管理控制器 - 处理令牌撤销和会话管理
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class TokenController : ControllerBase
    {
        private readonly ITokenManagementService _tokenService;
        private readonly ITokenBlacklistService? _blacklistService;

        public TokenController(
            ITokenManagementService tokenService,
            ITokenBlacklistService? blacklistService = null
        )
        {
            _tokenService = tokenService;
            _blacklistService = blacklistService;
        }

        /// <summary>
        /// 获取当前用户的活跃会话列表
        /// </summary>
        /// <returns>会话列表</returns>
        [HttpGet("sessions")]
        [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), 200)]
        public async Task<IActionResult> GetSessions()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var sessions = await _tokenService.GetUserSessionsAsync(userId.Value);

            // 标记当前会话
            var currentTokenId = GetCurrentTokenId();
            foreach (var session in sessions)
            {
                session.IsCurrent = session.SessionId.ToString() == currentTokenId;
            }

            return Ok(sessions);
        }

        /// <summary>
        /// 撤销指定会话/设备
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        [HttpDelete("sessions/{deviceId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RevokeSession(string deviceId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var count = await _tokenService.RevokeDeviceTokensAsync(userId.Value, deviceId);

            if (count == 0)
                return NotFound(new { message = "会话不存在或已失效" });

            return Ok(new { message = "会话已撤销", revokedCount = count });
        }

        /// <summary>
        /// 撤销当前用户的所有其他会话（仅保留当前会话）
        /// </summary>
        [HttpPost("sessions/revoke-others")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RevokeOtherSessions()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // 获取当前设备ID
            var currentDeviceId = GetCurrentDeviceId();

            // 获取所有会话
            var sessions = await _tokenService.GetUserSessionsAsync(userId.Value);
            var revokedCount = 0;

            foreach (var session in sessions)
            {
                if (session.DeviceId != currentDeviceId)
                {
                    await _tokenService.RevokeDeviceTokensAsync(userId.Value, session.DeviceId);
                    revokedCount++;
                }
            }

            return Ok(new { message = "其他会话已撤销", revokedCount });
        }

        /// <summary>
        /// 撤销当前用户的所有令牌（登出所有设备）
        /// </summary>
        [HttpPost("revoke-all")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RevokeAllTokens()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var count = await _tokenService.RevokeAllUserTokensAsync(userId.Value);

            // 同时加入用户黑名单
            if (_blacklistService != null)
            {
                await _blacklistService.BlacklistUserTokensAsync(
                    userId.Value,
                    DateTime.UtcNow.AddDays(30)
                );
            }

            return Ok(new { message = "所有令牌已撤销", revokedCount = count });
        }

        /// <summary>
        /// 获取登录策略设置
        /// </summary>
        [HttpGet("policy")]
        [ProducesResponseType(typeof(LoginPolicySettings), 200)]
        public async Task<IActionResult> GetLoginPolicy()
        {
            var policy = await _tokenService.GetLoginPolicyAsync();
            return Ok(policy);
        }

        /// <summary>
        /// 管理员：撤销指定用户的所有令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        [HttpPost("admin/revoke/{userId:guid}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> AdminRevokeUserTokens(Guid userId)
        {
            var count = await _tokenService.RevokeAllUserTokensAsync(userId);

            if (_blacklistService != null)
            {
                await _blacklistService.BlacklistUserTokensAsync(
                    userId,
                    DateTime.UtcNow.AddDays(30)
                );
            }

            return Ok(new { message = $"用户 {userId} 的所有令牌已撤销", revokedCount = count });
        }

        /// <summary>
        /// 管理员：获取指定用户的会话列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        [HttpGet("admin/sessions/{userId:guid}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), 200)]
        public async Task<IActionResult> AdminGetUserSessions(Guid userId)
        {
            var sessions = await _tokenService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }

        #region 辅助方法

        private Guid? GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        private string? GetCurrentTokenId()
        {
            return User.FindFirst("jti")?.Value;
        }

        private string GetCurrentDeviceId()
        {
            // 从请求头获取设备ID，或生成一个基于用户代理的标识
            var deviceId = Request.Headers["X-Device-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(deviceId))
                return deviceId;

            // 使用用户代理作为回退
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
            return Convert
                .ToBase64String(
                    System.Security.Cryptography.SHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(userAgent)
                    )
                )
                .Substring(0, 16);
        }

        #endregion
    }
}
