using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Token;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.Configuration;

namespace Dawning.Identity.Application.Services.Token
{
    /// <summary>
    /// Token management service implementation
    /// </summary>
    public class TokenManagementService : ITokenManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ITokenBlacklistService? _blacklistService;
        private LoginPolicySettings? _cachedPolicy;
        private DateTime _policyCacheTime;
        private static readonly TimeSpan PolicyCacheDuration = TimeSpan.FromMinutes(5);

        public TokenManagementService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ITokenBlacklistService? blacklistService = null
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _blacklistService = blacklistService;
        }

        /// <summary>
        /// Revoke all tokens for a user
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(Guid userId)
        {
            // Get all valid tokens for the user to add to blacklist
            var validTokens = await _unitOfWork.Token.GetValidTokensBySubjectAsync(
                userId.ToString()
            );
            var tokenList = validTokens.ToList();

            if (tokenList.Count == 0)
            {
                return 0;
            }

            // Revoke all tokens
            var affected = await _unitOfWork.Token.RevokeAllBySubjectAsync(userId.ToString());

            // Add tokens to blacklist (for fast validation)
            if (_blacklistService != null)
            {
                foreach (var token in tokenList)
                {
                    var expiration = token.ExpiresAt ?? DateTime.UtcNow.AddDays(30);
                    await _blacklistService.AddToBlacklistAsync(token.Id.ToString(), expiration);
                }
            }

            return affected;
        }

        /// <summary>
        /// Revoke a specified token
        /// </summary>
        public async Task<bool> RevokeTokenAsync(Guid tokenId)
        {
            // Get token information for blacklist
            var token = await _unitOfWork.Token.GetAsync(tokenId);
            if (token == null || token.Id == Guid.Empty || token.Status != "valid")
            {
                return false;
            }

            var expirationDate = token.ExpiresAt;

            // Revoke token
            var result = await _unitOfWork.Token.RevokeByIdAsync(tokenId);

            // Add to blacklist
            if (_blacklistService != null && result)
            {
                await _blacklistService.AddToBlacklistAsync(
                    tokenId.ToString(),
                    expirationDate ?? DateTime.UtcNow.AddDays(30)
                );
            }

            return result;
        }

        /// <summary>
        /// Revoke tokens for a specified device (not supported - user_sessions table does not exist)
        /// </summary>
        public Task<int> RevokeDeviceTokensAsync(Guid userId, string deviceId)
        {
            // user_sessions table does not exist, return 0
            return Task.FromResult(0);
        }

        /// <summary>
        /// Get user's active session list (not supported - user_sessions table does not exist)
        /// </summary>
        public Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId)
        {
            // user_sessions table does not exist, return empty list
            return Task.FromResult<IEnumerable<UserSessionDto>>(new List<UserSessionDto>());
        }

        /// <summary>
        /// Record user login session (not supported - user_sessions table does not exist)
        /// </summary>
        public Task RecordLoginSessionAsync(LoginSessionInfo session)
        {
            // user_sessions table does not exist, do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if login is allowed (based on device policy)
        /// </summary>
        public async Task<(bool allowed, string? message)> CheckLoginPolicyAsync(
            Guid userId,
            string deviceId
        )
        {
            var policy = await GetLoginPolicyAsync();

            // user_sessions table does not exist, allow login by default
            // If multiple devices not allowed, check if user already has valid tokens
            if (!policy.AllowMultipleDevices)
            {
                var validTokens = await _unitOfWork.Token.GetValidTokensBySubjectAsync(
                    userId.ToString()
                );
                if (validTokens.Any() && policy.NewDevicePolicy == "deny")
                {
                    return (false, "Simultaneous login from multiple devices is not allowed");
                }
            }

            return (true, null);
        }

        /// <summary>
        /// Get login policy settings
        /// </summary>
        public Task<LoginPolicySettings> GetLoginPolicyAsync()
        {
            if (_cachedPolicy != null && DateTime.UtcNow - _policyCacheTime < PolicyCacheDuration)
            {
                return Task.FromResult(_cachedPolicy);
            }

            var policy = new LoginPolicySettings();

            // Read configuration from appsettings.json
            var tokenSection = _configuration.GetSection("Security:Token");
            if (tokenSection.Exists())
            {
                policy.AllowMultipleDevices = tokenSection.GetValue("AllowMultipleDevices", true);
                policy.MaxDevices = tokenSection.GetValue("MaxDevices", 0);
                policy.NewDevicePolicy =
                    tokenSection.GetValue<string>("NewDevicePolicy") ?? "allow";
                policy.RefreshTokenLifetimeDays = tokenSection.GetValue(
                    "RefreshTokenLifetimeDays",
                    30
                );
                policy.AccessTokenLifetimeMinutes = tokenSection.GetValue(
                    "AccessTokenLifetimeMinutes",
                    60
                );
            }

            _cachedPolicy = policy;
            _policyCacheTime = DateTime.UtcNow;

            return Task.FromResult(policy);
        }
    }
}
