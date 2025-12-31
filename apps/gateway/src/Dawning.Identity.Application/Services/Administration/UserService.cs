using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Dtos.IntegrationEvents;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Core.Security;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// User Application Service Implementation
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPasswordPolicyService? _passwordPolicyService;
        private readonly IIntegrationEventBus _integrationEventBus;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IIntegrationEventBus integrationEventBus,
            ILogger<UserService> logger,
            IPasswordPolicyService? passwordPolicyService = null
        )
        {
            _userRepository = userRepository;
            _uow = uow;
            _mapper = mapper;
            _integrationEventBus = integrationEventBus;
            _logger = logger;
            _passwordPolicyService = passwordPolicyService;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        /// <summary>
        /// Get paged user list
        /// </summary>
        public async Task<PagedData<UserDto>> GetPagedListAsync(
            UserModel model,
            int page,
            int itemsPerPage
        )
        {
            var pagedData = await _userRepository.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<UserDto>
            {
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
                TotalCount = pagedData.TotalCount,
                Items = pagedData.Items.Select(u => _mapper.Map<UserDto>(u)),
            };
        }

        /// <summary>
        /// Get user list (Cursor pagination)
        /// </summary>
        public async Task<CursorPagedData<UserDto>> GetPagedListByCursorAsync(
            UserModel model,
            long? cursor,
            int pageSize
        )
        {
            var pagedData = await _userRepository.GetPagedListByCursorAsync(
                model,
                cursor,
                pageSize
            );

            return new CursorPagedData<UserDto>
            {
                PageSize = pagedData.PageSize,
                HasNextPage = pagedData.HasNextPage,
                NextCursor = pagedData.NextCursor,
                Items = pagedData.Items.Select(u => _mapper.Map<UserDto>(u)).ToList(),
            };
        }

        /// <summary>
        /// Create user
        /// </summary>
        public async Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId = null)
        {
            // Validate if username already exists
            if (await _userRepository.UsernameExistsAsync(dto.Username))
            {
                throw new InvalidOperationException($"Username '{dto.Username}' already exists.");
            }

            // Validate if email already exists
            if (
                !string.IsNullOrWhiteSpace(dto.Email)
                && await _userRepository.EmailExistsAsync(dto.Email)
            )
            {
                throw new InvalidOperationException($"Email '{dto.Email}' already exists.");
            }

            // Validate password complexity (prefer password policy service)
            await ValidatePasswordAsync(dto.Password);

            // 创建用户实体
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DisplayName = dto.DisplayName ?? dto.Username,
                Avatar = dto.Avatar,
                Role = dto.Role,
                IsActive = dto.IsActive,
                IsSystem = dto.IsSystem,
                Remark = dto.Remark,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId,
            };

            await _userRepository.InsertAsync(user);

            // Publish user creation integration event
            try
            {
                await _integrationEventBus.PublishAsync(
                    new UserEventIntegrationEvent
                    {
                        EventType = "UserCreated",
                        UserId = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Metadata = new Dictionary<string, object>
                        {
                            ["displayName"] = user.DisplayName ?? "",
                            ["role"] = user.Role ?? "",
                            ["createdBy"] = operatorId?.ToString() ?? "",
                        },
                    }
                );
                _logger.LogInformation(
                    "Published UserCreated integration event for user {UserId}",
                    user.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to publish UserCreated integration event for user {UserId}",
                    user.Id
                );
            }

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Update user
        /// </summary>
        public async Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid? operatorId = null)
        {
            var user = await _userRepository.GetAsync(dto.Id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{dto.Id}' not found.");
            }

            // Validate if email already exists (exclude current user)
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userRepository.EmailExistsAsync(dto.Email, user.Id))
                {
                    throw new InvalidOperationException($"Email '{dto.Email}' already exists.");
                }
            }

            // Update fields
            if (dto.Email != null)
                user.Email = dto.Email;
            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;
            if (dto.DisplayName != null)
                user.DisplayName = dto.DisplayName;
            if (dto.Avatar != null)
                user.Avatar = dto.Avatar;
            if (dto.Role != null)
                user.Role = dto.Role;
            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;
            if (dto.Remark != null)
                user.Remark = dto.Remark;

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = operatorId;

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, Guid? operatorId = null)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{id}' not found.");
            }

            // Protect system user, prevent deletion
            if (user.IsSystem)
            {
                throw new InvalidOperationException("System user cannot be deleted.");
            }

            user.UpdatedBy = operatorId;
            var result = await _userRepository.DeleteAsync(user);

            // Publish user deletion integration event
            if (result)
            {
                try
                {
                    await _integrationEventBus.PublishAsync(
                        new UserEventIntegrationEvent
                        {
                            EventType = "UserDeleted",
                            UserId = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Metadata = new Dictionary<string, object>
                            {
                                ["deletedBy"] = operatorId?.ToString() ?? "",
                            },
                        }
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to publish UserDeleted integration event for user {UserId}",
                        user.Id
                    );
                }
            }

            return result;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _userRepository.GetAsync(dto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{dto.UserId}' not found.");
            }

            // 验证旧密码
            if (!VerifyPassword(dto.OldPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Old password is incorrect.");
            }

            // 验证新密码与旧密码不同
            if (dto.NewPassword == dto.OldPassword)
            {
                throw new InvalidOperationException("新密码不能与旧密码相同。");
            }

            // 验证密码复杂度
            await ValidatePasswordAsync(dto.NewPassword);

            // 更新密码
            user.PasswordHash = HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);

            return result;
        }

        /// <summary>
        /// 重置密码（管理员功能）
        /// </summary>
        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            // 验证密码复杂度
            await ValidatePasswordAsync(newPassword);

            // 直接更新密码，不需要验证旧密码
            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);

            return result;
        }

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null)
        {
            return await _userRepository.UsernameExistsAsync(username, excludeUserId);
        }

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            return await _userRepository.EmailExistsAsync(email, excludeUserId);
        }

        #region 密码哈希和验证辅助方法

        /// <summary>
        /// 哈希密码（使用PBKDF2）
        /// </summary>
        private static string HashPassword(string password)
        {
            return PasswordHasher.Hash(password);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        private static bool VerifyPassword(string password, string passwordHash)
        {
            return PasswordHasher.Verify(password, passwordHash);
        }

        /// <summary>
        /// 验证密码复杂度
        /// </summary>
        /// <param name="password">待验证的密码</param>
        /// <returns>验证结果和错误消息</returns>
        private static (bool IsValid, string ErrorMessage) ValidatePasswordComplexity(
            string password
        )
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "密码不能为空");
            }

            if (password.Length < 8)
            {
                return (false, "密码长度至少为8个字符");
            }

            if (password.Length > 128)
            {
                return (false, "密码长度不能超过128个字符");
            }

            // 至少包含一个大写字母
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "密码必须包含至少一个大写字母");
            }

            // 至少包含一个小写字母
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "密码必须包含至少一个小写字母");
            }

            // 至少包含一个数字
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "密码必须包含至少一个数字");
            }

            // 至少包含一个特殊字符
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
            {
                return (false, "密码必须包含至少一个特殊字符 (!@#$%^&*等)");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 异步验证密码（优先使用密码策略服务，回退到本地验证）
        /// </summary>
        private async Task ValidatePasswordAsync(string password)
        {
            if (_passwordPolicyService != null)
            {
                var result = await _passwordPolicyService.ValidatePasswordAsync(password);
                if (!result.IsValid)
                {
                    var errorMessage =
                        result.Errors.Count > 0
                            ? string.Join("; ", result.Errors)
                            : "密码不符合策略要求";
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                // 回退到静态验证方法
                var (isValid, errorMessage) = ValidatePasswordComplexity(password);
                if (!isValid)
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        #endregion

        /// <summary>
        /// 更新最后登录时间
        /// </summary>
        public async Task UpdateLastLoginAsync(Guid userId)
        {
            // 直接通过Repository更新，不使用UnitOfWork事务
            // 这是一个独立的操作，不需要与其他操作在同一事务中
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                return;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        /// <summary>
        /// 验证用户密码
        /// </summary>
        public async Task<UserDto?> ValidatePasswordAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            // 从数据库获取用户（包含PasswordHash）
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // 验证密码 - 使用 PasswordHasher.Verify
            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                return null;
            }

            // 返回DTO（不包含密码）
            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 验证用户凭据并更新登录时间
        /// </summary>
        public async Task<UserDto?> ValidateCredentialsAndUpdateLoginAsync(
            string username,
            string password
        )
        {
            var user = await ValidatePasswordAsync(username, password);
            if (user != null)
            {
                // 在独立事务中更新最后登录时间，避免影响主事务
                try
                {
                    await UpdateLastLoginAsync(user.Id);
                }
                catch
                {
                    // 记录错误但不影响登录流程
                    // 可以使用日志记录: _logger.LogWarning(ex, "Failed to update last login time for user {UserId}", user.Id);
                }
            }
            return user;
        }

        /// <summary>
        /// 获取用户的所有角色
        /// </summary>
        public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId)
        {
            var roles = await _uow.UserRole.GetUserRolesAsync(userId);
            return roles.Select(r => _mapper.Map<RoleDto>(r));
        }

        /// <summary>
        /// 获取用户详情（含角色）
        /// </summary>
        public async Task<UserWithRolesDto?> GetUserWithRolesAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await GetUserRolesAsync(userId);

            var userWithRoles = _mapper.Map<UserWithRolesDto>(user);
            userWithRoles.Roles = roles.ToList();

            return userWithRoles;
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        public async Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        )
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            return await _uow.UserRole.AssignRolesAsync(userId, roleIds, operatorId);
        }

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
        {
            return await _uow.UserRole.RemoveRoleAsync(userId, roleId);
        }

        /// <summary>
        /// 获取用户统计信息
        /// </summary>
        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            // 获取所有用户用于统计（使用空模型获取全部）
            var allUsers = await _userRepository.GetPagedListAsync(
                new UserModel(),
                1,
                int.MaxValue
            );
            var users = allUsers.Items.ToList();

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var stats = new UserStatisticsDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                TodayLoginUsers = users.Count(u =>
                    u.LastLoginAt.HasValue && u.LastLoginAt.Value >= todayStart
                ),
                WeekLoginUsers = users.Count(u =>
                    u.LastLoginAt.HasValue && u.LastLoginAt.Value >= weekStart
                ),
                MonthLoginUsers = users.Count(u =>
                    u.LastLoginAt.HasValue && u.LastLoginAt.Value >= monthStart
                ),
                NeverLoginUsers = users.Count(u => !u.LastLoginAt.HasValue),
                GeneratedAt = now,
            };

            // 按角色统计
            var roleGroups = users
                .GroupBy(u => u.Role ?? "unknown")
                .ToDictionary(g => g.Key, g => g.Count());
            stats.UsersByRole = roleGroups;

            return stats;
        }

        /// <summary>
        /// 获取最近活跃用户（基于最后登录时间）
        /// </summary>
        public async Task<IEnumerable<RecentActiveUserDto>> GetRecentActiveUsersAsync(
            int count = 10
        )
        {
            // 获取有登录记录的用户，按最后登录时间降序
            var model = new UserModel { IsActive = true };
            var allUsers = await _userRepository.GetPagedListAsync(model, 1, int.MaxValue);

            var recentUsers = allUsers
                .Items.Where(u => u.LastLoginAt.HasValue)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(count)
                .Select(u => new RecentActiveUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    LastLoginAt = u.LastLoginAt,
                });

            return recentUsers;
        }
    }
}
