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
    /// Token management controller - handles token revocation and session management
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        /// Get active session list for current user
        /// </summary>
        /// <returns>Session list</returns>
        [HttpGet("sessions")]
        [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), 200)]
        public async Task<IActionResult> GetSessions()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var sessions = await _tokenService.GetUserSessionsAsync(userId.Value);

            // Mark current session
            var currentTokenId = GetCurrentTokenId();
            foreach (var session in sessions)
            {
                session.IsCurrent = session.SessionId.ToString() == currentTokenId;
            }

            return Ok(sessions);
        }

        /// <summary>
        /// Revoke specified session/device
        /// </summary>
        /// <param name="deviceId">Device ID</param>
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
                return NotFound(new { message = "Session does not exist or has expired" });

            return Ok(new { message = "Session revoked", revokedCount = count });
        }

        /// <summary>
        /// Revoke all other sessions for current user (keep only current session)
        /// </summary>
        [HttpPost("sessions/revoke-others")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RevokeOtherSessions()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // Get current device ID
            var currentDeviceId = GetCurrentDeviceId();

            // Get all sessions
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

            return Ok(new { message = "Other sessions revoked", revokedCount });
        }

        /// <summary>
        /// Revoke all tokens for current user (logout from all devices)
        /// </summary>
        [HttpPost("revoke-all")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RevokeAllTokens()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var count = await _tokenService.RevokeAllUserTokensAsync(userId.Value);

            // Also add user to blacklist
            if (_blacklistService != null)
            {
                await _blacklistService.BlacklistUserTokensAsync(
                    userId.Value,
                    DateTime.UtcNow.AddDays(30)
                );
            }

            return Ok(new { message = "All tokens revoked", revokedCount = count });
        }

        /// <summary>
        /// Get login policy settings
        /// </summary>
        [HttpGet("policy")]
        [ProducesResponseType(typeof(LoginPolicySettings), 200)]
        public async Task<IActionResult> GetLoginPolicy()
        {
            var policy = await _tokenService.GetLoginPolicyAsync();
            return Ok(policy);
        }

        /// <summary>
        /// Admin: Revoke all tokens for specified user
        /// </summary>
        /// <param name="userId">User ID</param>
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

            return Ok(new { message = $"All tokens for user {userId} have been revoked", revokedCount = count });
        }

        /// <summary>
        /// Admin: Get session list for specified user
        /// </summary>
        /// <param name="userId">User ID</param>
        [HttpGet("admin/sessions/{userId:guid}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), 200)]
        public async Task<IActionResult> AdminGetUserSessions(Guid userId)
        {
            var sessions = await _tokenService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }

        #region Helper Methods

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
            // Get device ID from request header, or generate an identifier based on user agent
            var deviceId = Request.Headers["X-Device-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(deviceId))
                return deviceId;

            // Use user agent as fallback
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
