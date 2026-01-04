using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.ORM.Dapper;
using static Dawning.ORM.Dapper.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// User repository implementation
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _context;

        public UserRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously get user by ID
        /// </summary>
        public async Task<User?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<UserEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            var result = await _context
                .Connection.Builder<UserEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(username), u => u.Username == username)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            var result = await _context
                .Connection.Builder<UserEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(email), u => u.Email == email)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// Get paged user list
        /// </summary>
        public async Task<PagedData<User>> GetPagedListAsync(
            UserModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            // Apply filter conditions
            builder = builder
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Username),
                    u => u.Username!.Contains(model.Username ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Email),
                    u => u.Email!.Contains(model.Email ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.DisplayName),
                    u => u.DisplayName!.Contains(model.DisplayName ?? "")
                )
                .WhereIf(!string.IsNullOrWhiteSpace(model.Role), u => u.Role == model.Role)
                .WhereIf(model.IsActive.HasValue, u => u.IsActive == model.IsActive!.Value);

            // Order by timestamp descending (for pagination optimization)
            var result = await builder
                .OrderByDescending(u => u.Timestamp)
                .AsPagedListAsync(page, itemsPerPage);

            return new PagedData<User>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };
        }

        /// <summary>
        /// Get user list (Cursor pagination)
        /// </summary>
        public async Task<CursorPagedData<User>> GetPagedListByCursorAsync(
            UserModel model,
            long? cursor,
            int pageSize
        )
        {
            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            // Apply filter conditions
            builder = builder
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Username),
                    u => u.Username!.Contains(model.Username ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Email),
                    u => u.Email!.Contains(model.Email ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.DisplayName),
                    u => u.DisplayName!.Contains(model.DisplayName ?? "")
                )
                .WhereIf(!string.IsNullOrWhiteSpace(model.Role), u => u.Role == model.Role)
                .WhereIf(model.IsActive.HasValue, u => u.IsActive == model.IsActive!.Value);

            // If cursor is provided, add cursor condition
            if (cursor.HasValue)
            {
                builder = builder.Where(u => u.Timestamp < cursor.Value);
            }

            // Order by Timestamp descending, get specified count + 1 (to determine if there is a next page)
            var items = builder.OrderByDescending(u => u.Timestamp).Take(pageSize + 1).AsList();

            var hasNextPage = items.Count() > pageSize;
            var resultItems = items.Take(pageSize).ToModels().ToList();
            var nextCursor =
                hasNextPage && resultItems.Any() ? resultItems.Last().Timestamp : (long?)null;

            return new CursorPagedData<User>
            {
                PageSize = pageSize,
                HasNextPage = hasNextPage,
                NextCursor = nextCursor,
                Items = resultItems,
            };
        }

        /// <summary>
        /// Asynchronously insert user
        /// </summary>
        public async ValueTask<int> InsertAsync(User model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Ensure created time is set
            if (model.CreatedAt == default)
            {
                model.CreatedAt = DateTime.UtcNow;
            }

            var entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Asynchronously update user
        /// </summary>
        public async ValueTask<bool> UpdateAsync(User model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Set update time and timestamp
            model.UpdatedAt = DateTime.UtcNow;
            model.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var entity = model.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Asynchronously delete user
        /// </summary>
        public async ValueTask<bool> DeleteAsync(User model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return await _context.Connection.DeleteAsync(model);
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            if (excludeUserId.HasValue)
            {
                var excludeId = excludeUserId.Value;
                builder = builder.WhereIf(true, u => u.Username == username && u.Id != excludeId);
            }
            else
            {
                builder = builder.WhereIf(true, u => u.Username == username);
            }

            var result = await builder.AsListAsync();
            return result.Any();
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            if (excludeUserId.HasValue)
            {
                var excludeId = excludeUserId.Value;
                builder = builder.WhereIf(true, u => u.Email == email && u.Id != excludeId);
            }
            else
            {
                builder = builder.WhereIf(true, u => u.Email == email);
            }

            var result = await builder.AsListAsync();
            return result.Any();
        }

        /// <summary>
        /// Get user lockout end time (if in lockout state)
        /// </summary>
        public async Task<DateTime?> GetLockoutEndAsync(string username)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || !user.LockoutEnabled || user.LockoutEnd == null)
            {
                return null;
            }

            return user.LockoutEnd > DateTime.UtcNow ? user.LockoutEnd : null;
        }

        /// <summary>
        /// Record failed login and return updated status
        /// </summary>
        public async Task<(
            int FailedCount,
            bool IsLockedOut,
            DateTime? LockoutEnd
        )> RecordFailedLoginAsync(
            string username,
            int maxFailedAttempts,
            int lockoutDurationMinutes
        )
        {
            var user = await GetByUsernameAsync(username);
            if (user == null)
            {
                return (0, false, null);
            }

            user.FailedLoginCount++;
            user.UpdatedAt = DateTime.UtcNow;

            // Check if lockout is needed
            if (user.FailedLoginCount >= maxFailedAttempts && user.LockoutEnabled)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
            }

            await UpdateAsync(user);

            var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
            return (user.FailedLoginCount, isLockedOut, user.LockoutEnd);
        }

        /// <summary>
        /// Reset failed login count
        /// </summary>
        public async Task ResetFailedLoginCountAsync(string username)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null)
            {
                return;
            }

            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(user);
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        public async Task UnlockUserAsync(Guid userId)
        {
            var user = await GetAsync(userId);
            if (user == null)
            {
                return;
            }

            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(user);
        }
    }
}
