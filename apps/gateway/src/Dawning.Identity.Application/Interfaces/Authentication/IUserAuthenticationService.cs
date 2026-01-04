using Dawning.Identity.Application.Dtos.Authentication;

namespace Dawning.Identity.Application.Interfaces.Authentication
{
    /// <summary>
    /// User authentication service interface
    /// </summary>
    public interface IUserAuthenticationService
    {
        /// <summary>
        /// Validate user credentials (username and password)
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Returns user information on success, null on failure</returns>
        Task<UserAuthenticationDto?> ValidateCredentialsAsync(string username, string password);

        /// <summary>
        /// Get user information by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User information</returns>
        Task<UserAuthenticationDto?> GetUserByIdAsync(string userId);
    }
}
