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

            // Create user entity
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
        /// Change password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _userRepository.GetAsync(dto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{dto.UserId}' not found.");
            }

            // Verify old password
            if (!VerifyPassword(dto.OldPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Old password is incorrect.");
            }

            // Verify new password differs from old password
            if (dto.NewPassword == dto.OldPassword)
            {
                throw new InvalidOperationException("New password cannot be the same as the old password.");
            }

            // Validate password complexity
            await ValidatePasswordAsync(dto.NewPassword);

            // Update password
            user.PasswordHash = HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);

            return result;
        }

        /// <summary>
        /// Reset password (admin function)
        /// </summary>
        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            // Validate password complexity
            await ValidatePasswordAsync(newPassword);

            // Update password directly without verifying old password
            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);

            return result;
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null)
        {
            return await _userRepository.UsernameExistsAsync(username, excludeUserId);
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            return await _userRepository.EmailExistsAsync(email, excludeUserId);
        }

        #region Password Hashing and Verification Helper Methods

        /// <summary>
        /// Hash password (using PBKDF2)
        /// </summary>
        private static string HashPassword(string password)
        {
            return PasswordHasher.Hash(password);
        }

        /// <summary>
        /// Verify password
        /// </summary>
        private static bool VerifyPassword(string password, string passwordHash)
        {
            return PasswordHasher.Verify(password, passwordHash);
        }

        /// <summary>
        /// Validate password complexity
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Validation result and error message</returns>
        private static (bool IsValid, string ErrorMessage) ValidatePasswordComplexity(
            string password
        )
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password cannot be empty");
            }

            if (password.Length < 8)
            {
                return (false, "Password must be at least 8 characters long");
            }

            if (password.Length > 128)
            {
                return (false, "Password cannot exceed 128 characters");
            }

            // Must contain at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter");
            }

            // Must contain at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter");
            }

            // Must contain at least one digit
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "Password must contain at least one digit");
            }

            // Must contain at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
            {
                return (false, "Password must contain at least one special character (!@#$%^&* etc.)");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate password asynchronously (prefer password policy service, fallback to local validation)
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
                            : "Password does not meet policy requirements";
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                // Fallback to static validation method
                var (isValid, errorMessage) = ValidatePasswordComplexity(password);
                if (!isValid)
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        #endregion

        /// <summary>
        /// Update last login time
        /// </summary>
        public async Task UpdateLastLoginAsync(Guid userId)
        {
            // Update directly through Repository without UnitOfWork transaction
            // This is an independent operation that doesn't need to be in the same transaction
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                return;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        /// <summary>
        /// Validate user password
        /// </summary>
        public async Task<UserDto?> ValidatePasswordAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            // Get user from database (including PasswordHash)
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Verify password - using PasswordHasher.Verify
            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                return null;
            }

            // Return DTO (without password)
            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Validate user credentials and update login time
        /// </summary>
        public async Task<UserDto?> ValidateCredentialsAndUpdateLoginAsync(
            string username,
            string password
        )
        {
            var user = await ValidatePasswordAsync(username, password);
            if (user != null)
            {
                // Update last login time in separate transaction to avoid affecting main transaction
                try
                {
                    await UpdateLastLoginAsync(user.Id);
                }
                catch
                {
                    // Log error but don't affect login flow
                    // Can use logger: _logger.LogWarning(ex, "Failed to update last login time for user {UserId}", user.Id);
                }
            }
            return user;
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId)
        {
            var roles = await _uow.UserRole.GetUserRolesAsync(userId);
            return roles.Select(r => _mapper.Map<RoleDto>(r));
        }

        /// <summary>
        /// Get user details with roles
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
        /// Assign roles to user
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
        /// Remove role from user
        /// </summary>
        public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
        {
            return await _uow.UserRole.RemoveRoleAsync(userId, roleId);
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            // Get all users for statistics (use empty model to get all)
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

            // Statistics by role
            var roleGroups = users
                .GroupBy(u => u.Role ?? "unknown")
                .ToDictionary(g => g.Key, g => g.Count());
            stats.UsersByRole = roleGroups;

            return stats;
        }

        /// <summary>
        /// Get recent active users (based on last login time)
        /// </summary>
        public async Task<IEnumerable<RecentActiveUserDto>> GetRecentActiveUsersAsync(
            int count = 10
        )
        {
            // Get users with login records, ordered by last login time descending
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
