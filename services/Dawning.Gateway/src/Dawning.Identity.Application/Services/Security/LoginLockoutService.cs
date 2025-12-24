using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.Configuration;

namespace Dawning.Identity.Application.Services.Security
{
    /// <summary>
    /// 登录锁定服务实现
    /// </summary>
    public class LoginLockoutService : ILoginLockoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private LockoutSettings? _cachedSettings;
        private DateTime _settingsCacheTime;
        private static readonly TimeSpan SettingsCacheDuration = TimeSpan.FromMinutes(5);

        public LoginLockoutService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        /// <summary>
        /// 检查用户是否被锁定
        /// </summary>
        public async Task<DateTime?> IsLockedOutAsync(string username)
        {
            var settings = await GetLockoutSettingsAsync();
            if (!settings.Enabled)
            {
                return null;
            }

            return await _unitOfWork.User.GetLockoutEndAsync(username);
        }

        /// <summary>
        /// 记录登录失败
        /// </summary>
        public async Task<(
            int failedCount,
            bool isLockedOut,
            DateTime? lockoutEnd
        )> RecordFailedLoginAsync(string username)
        {
            var settings = await GetLockoutSettingsAsync();
            if (!settings.Enabled)
            {
                return (0, false, null);
            }

            return await _unitOfWork.User.RecordFailedLoginAsync(
                username,
                settings.MaxFailedAttempts,
                settings.LockoutDurationMinutes
            );
        }

        /// <summary>
        /// 重置登录失败计数
        /// </summary>
        public async Task ResetFailedCountAsync(string username)
        {
            await _unitOfWork.User.ResetFailedLoginCountAsync(username);
        }

        /// <summary>
        /// 解锁用户账户
        /// </summary>
        public async Task UnlockUserAsync(Guid userId)
        {
            await _unitOfWork.User.UnlockUserAsync(userId);
        }

        /// <summary>
        /// 获取锁定配置
        /// </summary>
        public Task<LockoutSettings> GetLockoutSettingsAsync()
        {
            // 使用简单缓存避免频繁读取配置
            if (
                _cachedSettings != null
                && DateTime.UtcNow - _settingsCacheTime < SettingsCacheDuration
            )
            {
                return Task.FromResult(_cachedSettings);
            }

            var settings = new LockoutSettings();

            // 从 appsettings.json 读取配置
            var securitySection = _configuration.GetSection("Security:Lockout");
            if (securitySection.Exists())
            {
                settings.Enabled = securitySection.GetValue("Enabled", true);
                settings.MaxFailedAttempts = securitySection.GetValue("MaxFailedAttempts", 5);
                settings.LockoutDurationMinutes = securitySection.GetValue(
                    "LockoutDurationMinutes",
                    30
                );
            }

            _cachedSettings = settings;
            _settingsCacheTime = DateTime.UtcNow;

            return Task.FromResult(settings);
        }
    }
}
