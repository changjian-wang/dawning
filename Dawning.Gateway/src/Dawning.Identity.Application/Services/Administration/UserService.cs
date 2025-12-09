using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// 用户应用服务实现
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        /// <summary>
        /// 获取分页用户列表
        /// </summary>
        public async Task<PagedData<UserDto>> GetPagedListAsync(UserModel model, int page, int itemsPerPage)
        {
            var pagedData = await _userRepository.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<UserDto>
            {
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
                TotalCount = pagedData.TotalCount,
                Items = pagedData.Items.Select(u => _mapper.Map<UserDto>(u))
            };
        }

        /// <summary>
        /// 获取用户列表（Cursor 分页）
        /// </summary>
        public async Task<CursorPagedData<UserDto>> GetPagedListByCursorAsync(UserModel model, long? cursor, int pageSize)
        {
            var pagedData = await _userRepository.GetPagedListByCursorAsync(model, cursor, pageSize);

            return new CursorPagedData<UserDto>
            {
                PageSize = pagedData.PageSize,
                HasNextPage = pagedData.HasNextPage,
                NextCursor = pagedData.NextCursor,
                Items = pagedData.Items.Select(u => _mapper.Map<UserDto>(u)).ToList()
            };
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        public async Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId = null)
        {
            // 验证用户名是否已存在
            if (await _userRepository.UsernameExistsAsync(dto.Username))
            {
                throw new InvalidOperationException($"Username '{dto.Username}' already exists.");
            }

            // 验证邮箱是否已存在
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _userRepository.EmailExistsAsync(dto.Email))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' already exists.");
            }

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
                Remark = dto.Remark,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId
            };

            await _userRepository.InsertAsync(user);

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid? operatorId = null)
        {
            var user = await _userRepository.GetAsync(dto.Id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{dto.Id}' not found.");
            }

            // 验证邮箱是否已存在（排除当前用户）
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userRepository.EmailExistsAsync(dto.Email, user.Id))
                {
                    throw new InvalidOperationException($"Email '{dto.Email}' already exists.");
                }
            }

            // 更新字段
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
            if (dto.Avatar != null) user.Avatar = dto.Avatar;
            if (dto.Role != null) user.Role = dto.Role;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
            if (dto.Remark != null) user.Remark = dto.Remark;

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = operatorId;

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, Guid? operatorId = null)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{id}' not found.");
            }

            user.UpdatedBy = operatorId;
            var result = await _userRepository.DeleteAsync(user);

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

        #region 密码哈希辅助方法

        /// <summary>
        /// 哈希密码（使用SHA256）
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        private static bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
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

            // 验证密码
            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                return null;
            }

            // 返回DTO（不包含密码）
            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 验证用户凭据并更新登录时间
        /// </summary>
        public async Task<UserDto?> ValidateCredentialsAndUpdateLoginAsync(string username, string password)
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
            if (user == null) return null;

            var roles = await GetUserRolesAsync(userId);

            var userWithRoles = _mapper.Map<UserWithRolesDto>(user);
            userWithRoles.Roles = roles.ToList();

            return userWithRoles;
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        public async Task<bool> AssignRolesAsync(Guid userId, IEnumerable<Guid> roleIds, Guid? operatorId = null)
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
    }
}
