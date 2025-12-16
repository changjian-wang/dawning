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
using Dawning.Shared.Dapper.Contrib;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// 用户仓储实现
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _context;

        public UserRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID异步获取用户
        /// </summary>
        public async Task<User?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<UserEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        /// <summary>
        /// 根据用户名获取用户
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
        /// 根据邮箱获取用户
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
        /// 获取分页用户列表
        /// </summary>
        public async Task<PagedData<User>> GetPagedListAsync(
            UserModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            // 应用过滤条件
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

            // 按timestamp降序排序（用于分页优化）
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
        /// 获取用户列表（Cursor 分页）
        /// </summary>
        public async Task<CursorPagedData<User>> GetPagedListByCursorAsync(
            UserModel model,
            long? cursor,
            int pageSize
        )
        {
            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            // 应用过滤条件
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

            // 如果提供了游标，添加游标条件
            if (cursor.HasValue)
            {
                builder = builder.Where(u => u.Timestamp < cursor.Value);
            }

            // 按 Timestamp 降序排序，获取指定数量 + 1（用于判断是否有下一页）
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
        /// 异步插入用户
        /// </summary>
        public async ValueTask<int> InsertAsync(User model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // 确保设置创建时间
            if (model.CreatedAt == default)
            {
                model.CreatedAt = DateTime.UtcNow;
            }

            var entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// 异步更新用户
        /// </summary>
        public async ValueTask<bool> UpdateAsync(User model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // 设置更新时间和timestamp
            model.UpdatedAt = DateTime.UtcNow;
            model.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var entity = model.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// 异步删除用户
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
        /// 检查用户名是否存在
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
        /// 检查邮箱是否存在
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
        /// 获取用户锁定结束时间（如果处于锁定状态）
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
        /// 记录登录失败并返回更新后的状态
        /// </summary>
        public async Task<(int FailedCount, bool IsLockedOut, DateTime? LockoutEnd)> RecordFailedLoginAsync(
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

            // 检查是否需要锁定
            if (user.FailedLoginCount >= maxFailedAttempts && user.LockoutEnabled)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
            }

            await UpdateAsync(user);

            var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
            return (user.FailedLoginCount, isLockedOut, user.LockoutEnd);
        }

        /// <summary>
        /// 重置登录失败计数
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
        /// 解锁用户账户
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
