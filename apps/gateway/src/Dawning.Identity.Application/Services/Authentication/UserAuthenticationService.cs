using Dawning.Identity.Application.Dtos.Authentication;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Authentication;
using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Domain.Core.Security;

namespace Dawning.Identity.Application.Services.Authentication
{
    /// <summary>
    /// User authentication service implementation - integrates UserService for database validation
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly ILoginLockoutService? _lockoutService;

        public UserAuthenticationService(
            IUserService userService,
            ILoginLockoutService? lockoutService = null
        )
        {
            _userService = userService;
            _lockoutService = lockoutService;
        }

        /// <summary>
        /// Validate user credentials
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

            // Check if account is locked
            if (_lockoutService != null)
            {
                var lockoutEnd = await _lockoutService.IsLockedOutAsync(username);
                if (lockoutEnd.HasValue)
                {
                    // Return special lockout status
                    return new UserAuthenticationDto
                    {
                        IsLockedOut = true,
                        LockoutEnd = lockoutEnd.Value,
                        LockoutMessage =
                            $"Account is locked, please try again after {lockoutEnd.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}",
                    };
                }
            }

            // Use ValidateCredentialsAndUpdateLoginAsync method, which validates password and updates login time simultaneously
            var user = await _userService.ValidateCredentialsAndUpdateLoginAsync(
                username,
                password
            );

            if (user == null)
            {
                // Record failed login
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
                            LockoutMessage =
                                $"Too many failed login attempts, account locked until {lockoutEndTime.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}",
                        };
                    }
                }
                return null;
            }

            // Login successful, reset failed count
            if (_lockoutService != null)
            {
                await _lockoutService.ResetFailedCountAsync(username);
            }

            // Load user roles
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            // Return authentication DTO
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
        /// Get user information by user ID
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

            // Load user roles
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
        /// Get user information by username
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

            // Load user roles
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
