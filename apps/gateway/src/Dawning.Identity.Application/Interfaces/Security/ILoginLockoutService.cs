using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Security
{
    /// <summary>
    /// Login lockout service interface
    /// </summary>
    public interface ILoginLockoutService
    {
        /// <summary>
        /// Check if user is locked out
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>If locked out, returns the lockout end time; otherwise returns null</returns>
        Task<DateTime?> IsLockedOutAsync(string username);

        /// <summary>
        /// Record failed login attempt
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Returns the current failed count and whether the user is locked out</returns>
        Task<(int failedCount, bool isLockedOut, DateTime? lockoutEnd)> RecordFailedLoginAsync(
            string username
        );

        /// <summary>
        /// Reset failed login count (called after successful login)
        /// </summary>
        /// <param name="username">Username</param>
        Task ResetFailedCountAsync(string username);

        /// <summary>
        /// Unlock user account
        /// </summary>
        /// <param name="userId">User ID</param>
        Task UnlockUserAsync(Guid userId);

        /// <summary>
        /// Get lockout settings
        /// </summary>
        Task<LockoutSettings> GetLockoutSettingsAsync();
    }

    /// <summary>
    /// Lockout settings
    /// </summary>
    public class LockoutSettings
    {
        /// <summary>
        /// Whether login lockout is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum failed attempts
        /// </summary>
        public int MaxFailedAttempts { get; set; } = 5;

        /// <summary>
        /// Lockout duration (minutes)
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;
    }
}
