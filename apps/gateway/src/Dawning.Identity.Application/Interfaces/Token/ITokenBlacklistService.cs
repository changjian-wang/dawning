using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Token
{
    /// <summary>
    /// Token blacklist service interface
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Adds a token to the blacklist
        /// </summary>
        /// <param name="jti">Token unique identifier (JWT ID)</param>
        /// <param name="expiration">Expiration time</param>
        Task AddToBlacklistAsync(string jti, DateTime expiration);

        /// <summary>
        /// Checks whether a token is in the blacklist
        /// </summary>
        /// <param name="jti">Token unique identifier</param>
        Task<bool> IsBlacklistedAsync(string jti);

        /// <summary>
        /// Adds all tokens of a user to the blacklist
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="expiration">Expiration time</param>
        Task BlacklistUserTokensAsync(Guid userId, DateTime expiration);

        /// <summary>
        /// Checks whether a user is in the global blacklist
        /// </summary>
        /// <param name="userId">User ID</param>
        Task<bool> IsUserBlacklistedAsync(Guid userId);

        /// <summary>
        /// Cleans up expired blacklist entries
        /// </summary>
        Task CleanupExpiredEntriesAsync();
    }
}
