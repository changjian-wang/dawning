using System;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// User repository interface
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Asynchronously get user by ID
        /// </summary>
        Task<User?> GetAsync(Guid id);

        /// <summary>
        /// Get user by username
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Get paginated user list
        /// </summary>
        Task<PagedData<User>> GetPagedListAsync(UserModel model, int page, int itemsPerPage);

        /// <summary>
        /// Get user list (cursor pagination)
        /// </summary>
        Task<CursorPagedData<User>> GetPagedListByCursorAsync(
            UserModel model,
            long? cursor,
            int pageSize
        );

        /// <summary>
        /// Asynchronously insert user
        /// </summary>
        ValueTask<int> InsertAsync(User model);

        /// <summary>
        /// Asynchronously update user
        /// </summary>
        ValueTask<bool> UpdateAsync(User model);

        /// <summary>
        /// Asynchronously delete user (soft delete)
        /// </summary>
        ValueTask<bool> DeleteAsync(User model);

        /// <summary>
        /// Check if username exists
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

        /// <summary>
        /// Check if email exists
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);

        /// <summary>
        /// Get user lockout end time (if in locked state)
        /// </summary>
        Task<DateTime?> GetLockoutEndAsync(string username);

        /// <summary>
        /// Record failed login attempt and return updated status
        /// </summary>
        Task<(int FailedCount, bool IsLockedOut, DateTime? LockoutEnd)> RecordFailedLoginAsync(
            string username,
            int maxFailedAttempts,
            int lockoutDurationMinutes
        );

        /// <summary>
        /// Reset failed login count
        /// </summary>
        Task ResetFailedLoginCountAsync(string username);

        /// <summary>
        /// Unlock user account
        /// </summary>
        Task UnlockUserAsync(Guid userId);
    }
}
