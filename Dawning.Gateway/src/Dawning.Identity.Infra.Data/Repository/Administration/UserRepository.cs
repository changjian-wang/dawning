using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var result = await _context.Connection.Builder<UserEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(username), u => u.Username == username && u.IsDeleted == false)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            var result = await _context.Connection.Builder<UserEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(email), u => u.Email == email && u.IsDeleted == false)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// 获取分页用户列表
        /// </summary>
        public async Task<PagedData<User>> GetPagedListAsync(UserModel model, int page, int itemsPerPage)
        {
            var builder = _context.Connection.Builder<UserEntity>(_context.Transaction);

            // 默认不包含已删除的用户
            builder = builder.WhereIf(!model.IncludeDeleted, u => u.IsDeleted == false);

            // 应用过滤条件
            builder = builder
                .WhereIf(!string.IsNullOrWhiteSpace(model.Username), u => u.Username!.Contains(model.Username ?? ""))
                .WhereIf(!string.IsNullOrWhiteSpace(model.Email), u => u.Email!.Contains(model.Email ?? ""))
                .WhereIf(!string.IsNullOrWhiteSpace(model.DisplayName), u => u.DisplayName!.Contains(model.DisplayName ?? ""))
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
                Items = result.Values.ToModels()
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
        /// 异步删除用户（软删除）
        /// </summary>
        public async ValueTask<bool> DeleteAsync(User model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // 软删除
            model.IsDeleted = true;
            model.UpdatedAt = DateTime.UtcNow;

            return await UpdateAsync(model);
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
                builder = builder.WhereIf(true, u => u.Username == username && u.IsDeleted == false && u.Id != excludeId);
            }
            else
            {
                builder = builder.WhereIf(true, u => u.Username == username && u.IsDeleted == false);
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
                builder = builder.WhereIf(true, u => u.Email == email && u.IsDeleted == false && u.Id != excludeId);
            }
            else
            {
                builder = builder.WhereIf(true, u => u.Email == email && u.IsDeleted == false);
            }

            var result = await builder.AsListAsync();
            return result.Any();
        }
    }
}
