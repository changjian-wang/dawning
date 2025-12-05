using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// 用户仓储接口
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 根据ID异步获取用户
        /// </summary>
        Task<User?> GetAsync(Guid id);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// 获取分页用户列表
        /// </summary>
        Task<PagedData<User>> GetPagedListAsync(UserModel model, int page, int itemsPerPage);

        /// <summary>
        /// 获取用户列表（游标分页）
        /// </summary>
        Task<CursorPagedData<User>> GetPagedListByCursorAsync(int pageSize, long? cursor = null);

        /// <summary>
        /// 异步插入用户
        /// </summary>
        ValueTask<int> InsertAsync(User model);

        /// <summary>
        /// 异步更新用户
        /// </summary>
        ValueTask<bool> UpdateAsync(User model);

        /// <summary>
        /// 异步删除用户（软删除）
        /// </summary>
        ValueTask<bool> DeleteAsync(User model);

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    }
}
