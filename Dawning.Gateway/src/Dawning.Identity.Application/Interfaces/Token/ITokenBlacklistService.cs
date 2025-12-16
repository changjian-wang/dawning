using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Token
{
    /// <summary>
    /// Token 黑名单服务接口
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// 将令牌加入黑名单
        /// </summary>
        /// <param name="jti">令牌唯一标识 (JWT ID)</param>
        /// <param name="expiration">过期时间</param>
        Task AddToBlacklistAsync(string jti, DateTime expiration);

        /// <summary>
        /// 检查令牌是否在黑名单中
        /// </summary>
        /// <param name="jti">令牌唯一标识</param>
        Task<bool> IsBlacklistedAsync(string jti);

        /// <summary>
        /// 将用户的所有令牌加入黑名单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="expiration">过期时间</param>
        Task BlacklistUserTokensAsync(Guid userId, DateTime expiration);

        /// <summary>
        /// 检查用户是否在全局黑名单中
        /// </summary>
        /// <param name="userId">用户ID</param>
        Task<bool> IsUserBlacklistedAsync(Guid userId);

        /// <summary>
        /// 清理过期的黑名单条目
        /// </summary>
        Task CleanupExpiredEntriesAsync();
    }
}
