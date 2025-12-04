using Dawning.Identity.Application.Dtos.Authentication;

namespace Dawning.Identity.Application.Interfaces.Authentication
{
    /// <summary>
    /// 用户认证服务接口
    /// </summary>
    public interface IUserAuthenticationService
    {
        /// <summary>
        /// 验证用户凭据（用户名和密码）
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>验证成功返回用户信息，失败返回 null</returns>
        Task<UserAuthenticationDto?> ValidateCredentialsAsync(string username, string password);

        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<UserAuthenticationDto?> GetUserByIdAsync(string userId);
    }
}
