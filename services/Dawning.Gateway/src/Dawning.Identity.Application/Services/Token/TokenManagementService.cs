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
    /// Token 管理服务实现
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
        /// 撤销用户的所有令牌
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(Guid userId)
        {
            // 获取用户的所有有效令牌用于黑名单
            var validTokens = await _unitOfWork.Token.GetValidTokensBySubjectAsync(
                userId.ToString()
            );
            var tokenList = validTokens.ToList();

            if (tokenList.Count == 0)
            {
                return 0;
            }

            // 撤销所有令牌
            var affected = await _unitOfWork.Token.RevokeAllBySubjectAsync(userId.ToString());

            // 将令牌加入黑名单（用于快速验证）
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
        /// 撤销指定令牌
        /// </summary>
        public async Task<bool> RevokeTokenAsync(Guid tokenId)
        {
            // 获取令牌信息用于黑名单
            var token = await _unitOfWork.Token.GetAsync(tokenId);
            if (token == null || token.Id == Guid.Empty || token.Status != "valid")
            {
                return false;
            }

            var expirationDate = token.ExpiresAt;

            // 撤销令牌
            var result = await _unitOfWork.Token.RevokeByIdAsync(tokenId);

            // 加入黑名单
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
        /// 撤销指定设备的令牌（暂不支持，user_sessions 表不存在）
        /// </summary>
        public Task<int> RevokeDeviceTokensAsync(Guid userId, string deviceId)
        {
            // user_sessions 表不存在，返回 0
            return Task.FromResult(0);
        }

        /// <summary>
        /// 获取用户的活跃会话列表（暂不支持，user_sessions 表不存在）
        /// </summary>
        public Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId)
        {
            // user_sessions 表不存在，返回空列表
            return Task.FromResult<IEnumerable<UserSessionDto>>(new List<UserSessionDto>());
        }

        /// <summary>
        /// 记录用户登录会话（暂不支持，user_sessions 表不存在）
        /// </summary>
        public Task RecordLoginSessionAsync(LoginSessionInfo session)
        {
            // user_sessions 表不存在，不做任何操作
            return Task.CompletedTask;
        }

        /// <summary>
        /// 检查是否允许登录（基于设备策略）
        /// </summary>
        public async Task<(bool allowed, string? message)> CheckLoginPolicyAsync(
            Guid userId,
            string deviceId
        )
        {
            var policy = await GetLoginPolicyAsync();

            // user_sessions 表不存在，默认允许登录
            // 如果不允许多设备，则检查用户是否已有有效令牌
            if (!policy.AllowMultipleDevices)
            {
                var validTokens = await _unitOfWork.Token.GetValidTokensBySubjectAsync(
                    userId.ToString()
                );
                if (validTokens.Any() && policy.NewDevicePolicy == "deny")
                {
                    return (false, "不允许在多个设备上同时登录");
                }
            }

            return (true, null);
        }

        /// <summary>
        /// 获取登录策略设置
        /// </summary>
        public Task<LoginPolicySettings> GetLoginPolicyAsync()
        {
            if (_cachedPolicy != null && DateTime.UtcNow - _policyCacheTime < PolicyCacheDuration)
            {
                return Task.FromResult(_cachedPolicy);
            }

            var policy = new LoginPolicySettings();

            // 从 appsettings.json 读取配置
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
