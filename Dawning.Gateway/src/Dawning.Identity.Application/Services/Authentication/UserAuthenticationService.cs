using Dawning.Identity.Application.Dtos.Authentication;
using Dawning.Identity.Application.Interfaces.Authentication;

namespace Dawning.Identity.Application.Services.Authentication
{
    /// <summary>
    /// 用户认证服务实现（临时实现，用于演示）
    /// TODO: 在生产环境中应该连接真实的用户数据库
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        // 模拟用户数据库（实际应该从数据库读取）
        private static readonly List<UserAuthenticationDto> _users = new()
        {
            new UserAuthenticationDto
            {
                Id = "1",
                Username = "admin",
                Email = "admin@dawning.com",
                Role = "admin",
                IsActive = true
            },
            new UserAuthenticationDto
            {
                Id = "2",
                Username = "user",
                Email = "user@dawning.com",
                Role = "user",
                IsActive = true
            }
        };

        // 模拟密码存储（实际应该使用加密存储）
        // 密码映射: admin -> admin, user -> user123
        private static readonly Dictionary<string, string> _passwords = new()
        {
            { "admin", "admin" },
            { "user", "user123" }
        };

        /// <summary>
        /// 验证用户凭据
        /// </summary>
        public Task<UserAuthenticationDto?> ValidateCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult<UserAuthenticationDto?>(null);
            }

            // 查找用户
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.IsActive);

            if (user == null)
            {
                return Task.FromResult<UserAuthenticationDto?>(null);
            }

            // 验证密码（实际应该使用加密比对）
            if (_passwords.TryGetValue(user.Username, out var storedPassword) &&
                storedPassword == password)
            {
                return Task.FromResult<UserAuthenticationDto?>(user);
            }

            return Task.FromResult<UserAuthenticationDto?>(null);
        }

        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        public Task<UserAuthenticationDto?> GetUserByIdAsync(string userId)
        {
            var user = _users.FirstOrDefault(u =>
                u.Id == userId &&
                u.IsActive);

            return Task.FromResult(user);
        }
    }
}
