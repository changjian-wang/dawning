using Dawning.Identity.Application.Dtos.Authentication;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Authentication;
using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Domain.Core.Security;

namespace Dawning.Identity.Application.Services.Authentication
{
    /// <summary>
    /// 用户认证服务实现 - 集成UserService进行数据库验证
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly ILoginLockoutService? _lockoutService;

        public UserAuthenticationService(
            IUserService userService,
            ILoginLockoutService? lockoutService = null)
        {
            _userService = userService;
            _lockoutService = lockoutService;
        }

        /// <summary>
        /// 验证用户凭据
        /// </summary>
        public async Task<UserAuthenticationDto?> ValidateCredentialsAsync(
            string username,
            string password
        )
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            // 检查账户是否被锁定
            if (_lockoutService != null)
            {
                var lockoutEnd = await _lockoutService.IsLockedOutAsync(username);
                if (lockoutEnd.HasValue)
                {
                    // 返回特殊的锁定状态
                    return new UserAuthenticationDto
                    {
                        IsLockedOut = true,
                        LockoutEnd = lockoutEnd.Value,
                        LockoutMessage = $"账户已被锁定，请在 {lockoutEnd.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss} 后重试"
                    };
                }
            }

            // 使用ValidateCredentialsAndUpdateLoginAsync方法，它会同时验证密码和更新登录时间
            var user = await _userService.ValidateCredentialsAndUpdateLoginAsync(
                username,
                password
            );

            if (user == null)
            {
                // 记录登录失败
                if (_lockoutService != null)
                {
                    var (failedCount, isLockedOut, lockoutEndTime) = 
                        await _lockoutService.RecordFailedLoginAsync(username);
                    
                    if (isLockedOut && lockoutEndTime.HasValue)
                    {
                        return new UserAuthenticationDto
                        {
                            IsLockedOut = true,
                            LockoutEnd = lockoutEndTime.Value,
                            LockoutMessage = $"登录失败次数过多，账户已被锁定至 {lockoutEndTime.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}"
                        };
                    }
                }
                return null;
            }

            // 登录成功，重置失败计数
            if (_lockoutService != null)
            {
                await _lockoutService.ResetFailedCountAsync(username);
            }

            // 加载用户角色
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            // 返回认证DTO
            return new UserAuthenticationDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Roles = roleNames,
                IsActive = user.IsActive,
            };
        }

        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        public async Task<UserAuthenticationDto?> GetUserByIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var id))
            {
                return null;
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // 加载用户角色
            var userRoles = await _userService.GetUserRolesAsync(id);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            return new UserAuthenticationDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Roles = roleNames,
                IsActive = user.IsActive,
            };
        }

        /// <summary>
        /// 根据用户名获取用户信息
        /// </summary>
        public async Task<UserAuthenticationDto?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var model = new Domain.Models.Administration.UserModel
            {
                Username = username,
                IsActive = true,
            };
            var users = await _userService.GetPagedListAsync(model, 1, 10);

            var user = users.Items.FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // 加载用户角色
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            return new UserAuthenticationDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Roles = roleNames,
                IsActive = user.IsActive,
            };
        }
    }
}
