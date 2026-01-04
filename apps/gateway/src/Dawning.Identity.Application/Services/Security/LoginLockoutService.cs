using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.Configuration;

namespace Dawning.Identity.Application.Services.Security
{
    /// <summary>
    /// Login lockout service implementation
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
        /// Check if user is locked out
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
        /// Record failed login attempt
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
        /// Reset failed login count
        /// </summary>
        public async Task ResetFailedCountAsync(string username)
        {
            await _unitOfWork.User.ResetFailedLoginCountAsync(username);
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        public async Task UnlockUserAsync(Guid userId)
        {
            await _unitOfWork.User.UnlockUserAsync(userId);
        }

        /// <summary>
        /// Get lockout configuration
        /// </summary>
        public Task<LockoutSettings> GetLockoutSettingsAsync()
        {
            // Use simple cache to avoid frequent config reads
            if (
                _cachedSettings != null
                && DateTime.UtcNow - _settingsCacheTime < SettingsCacheDuration
            )
            {
                return Task.FromResult(_cachedSettings);
            }

            var settings = new LockoutSettings();

            // Read configuration from appsettings.json
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
