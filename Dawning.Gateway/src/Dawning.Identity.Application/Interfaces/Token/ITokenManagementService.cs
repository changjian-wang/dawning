using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Token
{
    /// <summary>
    /// Token 管理服务接口
    /// </summary>
    public interface ITokenManagementService
    {
        /// <summary>
        /// 撤销用户的所有令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>撤销的令牌数量</returns>
        Task<int> RevokeAllUserTokensAsync(Guid userId);

        /// <summary>
        /// 撤销指定令牌
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        Task<bool> RevokeTokenAsync(Guid tokenId);

        /// <summary>
        /// 撤销指定设备的令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="deviceId">设备ID</param>
        Task<int> RevokeDeviceTokensAsync(Guid userId, string deviceId);

        /// <summary>
        /// 获取用户的活跃会话列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId);

        /// <summary>
        /// 记录用户登录会话
        /// </summary>
        Task RecordLoginSessionAsync(LoginSessionInfo session);

        /// <summary>
        /// 检查是否允许登录（基于设备策略）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="deviceId">设备ID</param>
        Task<(bool allowed, string? message)> CheckLoginPolicyAsync(Guid userId, string deviceId);

        /// <summary>
        /// 获取登录策略设置
        /// </summary>
        Task<LoginPolicySettings> GetLoginPolicyAsync();
    }

    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class UserSessionDto
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// 设备类型（web, mobile, desktop）
        /// </summary>
        public string DeviceType { get; set; } = "unknown";

        /// <summary>
        /// 设备名称/用户代理
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// IP 地址
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// 是否为当前会话
        /// </summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// 登录会话信息（用于记录）
    /// </summary>
    public class LoginSessionInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 令牌ID
        /// </summary>
        public Guid TokenId { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// 设备类型
        /// </summary>
        public string DeviceType { get; set; } = "web";

        /// <summary>
        /// 设备名称/用户代理
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// IP 地址
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录策略设置
    /// </summary>
    public class LoginPolicySettings
    {
        /// <summary>
        /// 是否允许多设备登录
        /// </summary>
        public bool AllowMultipleDevices { get; set; } = true;

        /// <summary>
        /// 最大允许设备数（0表示不限制）
        /// </summary>
        public int MaxDevices { get; set; } = 0;

        /// <summary>
        /// 新设备登录时的策略（allow, deny, kick_oldest）
        /// </summary>
        public string NewDevicePolicy { get; set; } = "allow";

        /// <summary>
        /// 刷新令牌有效期（天）
        /// </summary>
        public int RefreshTokenLifetimeDays { get; set; } = 30;

        /// <summary>
        /// 访问令牌有效期（分钟）
        /// </summary>
        public int AccessTokenLifetimeMinutes { get; set; } = 60;
    }
}
