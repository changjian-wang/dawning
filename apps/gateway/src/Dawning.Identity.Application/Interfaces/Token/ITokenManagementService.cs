using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Token
{
    /// <summary>
    /// Token management service interface
    /// </summary>
    public interface ITokenManagementService
    {
        /// <summary>
        /// Revokes all tokens of a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of revoked tokens</returns>
        Task<int> RevokeAllUserTokensAsync(Guid userId);

        /// <summary>
        /// Revokes a specified token
        /// </summary>
        /// <param name="tokenId">Token ID</param>
        Task<bool> RevokeTokenAsync(Guid tokenId);

        /// <summary>
        /// Revokes tokens of a specified device
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="deviceId">Device ID</param>
        Task<int> RevokeDeviceTokensAsync(Guid userId, string deviceId);

        /// <summary>
        /// Gets the list of active sessions for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId);

        /// <summary>
        /// Records a user login session
        /// </summary>
        Task RecordLoginSessionAsync(LoginSessionInfo session);

        /// <summary>
        /// Checks whether login is allowed (based on device policy)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="deviceId">Device ID</param>
        Task<(bool allowed, string? message)> CheckLoginPolicyAsync(Guid userId, string deviceId);

        /// <summary>
        /// Gets the login policy settings
        /// </summary>
        Task<LoginPolicySettings> GetLoginPolicyAsync();
    }

    /// <summary>
    /// User session information
    /// </summary>
    public class UserSessionDto
    {
        /// <summary>
        /// Session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Device ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Device type (web, mobile, desktop)
        /// </summary>
        public string DeviceType { get; set; } = "unknown";

        /// <summary>
        /// Device name/user agent
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// IP address
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Login time
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// Last active time
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Whether this is the current session
        /// </summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// Login session information (for recording)
    /// </summary>
    public class LoginSessionInfo
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Token ID
        /// </summary>
        public Guid TokenId { get; set; }

        /// <summary>
        /// Device ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Device type
        /// </summary>
        public string DeviceType { get; set; } = "web";

        /// <summary>
        /// Device name/user agent
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// IP address
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login policy settings
    /// </summary>
    public class LoginPolicySettings
    {
        /// <summary>
        /// Whether multiple device login is allowed
        /// </summary>
        public bool AllowMultipleDevices { get; set; } = true;

        /// <summary>
        /// Maximum allowed devices (0 means no limit)
        /// </summary>
        public int MaxDevices { get; set; } = 0;

        /// <summary>
        /// Policy for new device login (allow, deny, kick_oldest)
        /// </summary>
        public string NewDevicePolicy { get; set; } = "allow";

        /// <summary>
        /// Refresh token lifetime (days)
        /// </summary>
        public int RefreshTokenLifetimeDays { get; set; } = 30;

        /// <summary>
        /// Access token lifetime (minutes)
        /// </summary>
        public int AccessTokenLifetimeMinutes { get; set; } = 60;
    }
}
