using System.Security.Claims;
using Dapper;
using Dawning.Identity.Api.Helpers;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Authentication;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Dawning.Identity.Api.Controllers
{
    /// <summary>
    /// 用户管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly AuditLogHelper _auditLogHelper;

        public UserController(
            IUserAuthenticationService userAuthenticationService,
            IUserService userService,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<UserController> logger,
            AuditLogHelper auditLogHelper
        )
        {
            _userAuthenticationService = userAuthenticationService;
            _userService = userService;
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("info")]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            try
            {
                // 从 JWT Claims 中获取用户 ID
                var userId =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token claims");
                    return Unauthorized(
                        ApiResponse.Error(40100, "Invalid token: user ID not found")
                    );
                }

                // 获取用户信息
                var user = await _userAuthenticationService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(ApiResponse.Error(40400, "User not found"));
                }

                // 转换为响应 DTO
                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username!,
                    Email = user.Email!,
                    Roles = user.Roles,
                    Name = user.Username!, // 暂时使用 username 作为显示名称
                    Avatar = null, // 暂无头像功能
                    CreatedAt = DateTime.UtcNow, // 暂时返回当前时间
                    IsActive = user.IsActive,
                };

                _logger.LogInformation(
                    "User info retrieved successfully for user: {UserId}",
                    userId
                );
                return Ok(ApiResponse<object>.Success(userInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user info");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,super_admin,user_manager,auditor")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserList(
            [FromQuery] string? username,
            [FromQuery] string? email,
            [FromQuery] string? displayName,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var model = new UserModel
                {
                    Username = username,
                    Email = email,
                    DisplayName = displayName,
                    Role = role,
                    IsActive = isActive,
                };

                var result = await _userService.GetPagedListAsync(model, page, pageSize);

                _logger.LogInformation(
                    "User list retrieved: page {Page}, total {Total}",
                    page,
                    result.TotalCount
                );

                return Ok(
                    ApiResponse<object>.SuccessPaged(
                        result.Items,
                        result.PageIndex,
                        result.PageSize,
                        result.TotalCount
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 获取用户列表（Cursor 分页）
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserListByCursor(
            [FromQuery] string? username,
            [FromQuery] string? email,
            [FromQuery] string? displayName,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] long? cursor,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var model = new UserModel
                {
                    Username = username,
                    Email = email,
                    DisplayName = displayName,
                    Role = role,
                    IsActive = isActive,
                };

                var result = await _userService.GetPagedListByCursorAsync(model, cursor, pageSize);

                _logger.LogInformation(
                    "User list by cursor retrieved: pageSize {PageSize}, hasNextPage {HasNextPage}, cursor {Cursor}",
                    pageSize,
                    result.HasNextPage,
                    cursor
                );

                return Ok(
                    ApiResponse<object>.Success(
                        new
                        {
                            items = result.Items,
                            pageSize = result.PageSize,
                            hasNextPage = result.HasNextPage,
                            nextCursor = result.NextCursor,
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list by cursor");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 根据ID获取用户详情
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse.Error(40400, "User not found"));
                }

                return Ok(ApiResponse<object>.Success(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // 获取当前操作者ID
                var operatorId = GetCurrentUserId();

                var user = await _userService.CreateAsync(dto, operatorId);

                _logger.LogInformation("User created: {Username}", user.Username);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "Create",
                    entityType: "User",
                    entityId: user.Id,
                    description: $"Created user: {user.Username}",
                    newValues: new
                    {
                        user.Username,
                        user.Email,
                        user.Role,
                        user.IsActive,
                    },
                    statusCode: 201
                );

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = user.Id },
                    ApiResponse<object>.Success(user, "User created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create user: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                dto.Id = id; // 确保ID匹配

                // 获取当前操作者ID
                var operatorId = GetCurrentUserId();

                // 获取更新前的用户信息
                var oldUser = await _userRepository.GetAsync(id);
                var oldValues =
                    oldUser != null
                        ? new
                        {
                            oldUser.Username,
                            oldUser.Email,
                            oldUser.Role,
                            oldUser.IsActive,
                        }
                        : null;

                var user = await _userService.UpdateAsync(dto, operatorId);

                _logger.LogInformation("User updated: {UserId}", id);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"Updated user: {user.Username}",
                    oldValues: oldValues,
                    newValues: new
                    {
                        user.Username,
                        user.Email,
                        user.Role,
                        user.IsActive,
                    }
                );

                return Ok(ApiResponse<object>.Success(user, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update user: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                // 获取当前操作者ID
                var operatorId = GetCurrentUserId();

                // 获取被删除的用户信息
                var user = await _userRepository.GetAsync(id);
                var userInfo = user != null ? new { user.Username, user.Email } : null;

                var result = await _userService.DeleteAsync(id, operatorId);

                _logger.LogInformation("User deleted: {UserId}", id);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "Delete",
                    entityType: "User",
                    entityId: id,
                    description: $"Deleted user: {userInfo?.Username}",
                    oldValues: userInfo
                );

                return Ok(ApiResponse.Success("User deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to delete user: {Message}", ex.Message);
                return NotFound(ApiResponse.Error(40400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var result = await _userService.ChangePasswordAsync(dto);

                _logger.LogInformation("Password changed for user: {UserId}", dto.UserId);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "ChangePassword",
                    entityType: "User",
                    entityId: dto.UserId,
                    description: "User changed their password"
                );

                return Ok(ApiResponse.Success("Password changed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to change password: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 重置密码（管理员功能）
        /// </summary>
        [HttpPost("{id:guid}/reset-password")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword(
            Guid id,
            [FromBody] ResetPasswordRequest request
        )
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(id, request.NewPassword);

                _logger.LogInformation("Password reset for user: {UserId}", id);

                // 获取用户信息
                var user = await _userRepository.GetAsync(id);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "ResetPassword",
                    entityType: "User",
                    entityId: id,
                    description: $"Admin reset password for user: {user?.Username}"
                );

                return Ok(ApiResponse.Success("Password reset successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to reset password: {Message}", ex.Message);
                return NotFound(ApiResponse.Error(40400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        [HttpGet("check-username")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUsername(
            [FromQuery] string username,
            [FromQuery] Guid? excludeUserId
        )
        {
            try
            {
                var exists = await _userService.UsernameExistsAsync(username, excludeUserId);
                return Ok(ApiResponse<object>.Success(new { exists }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        [HttpGet("check-email")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail(
            [FromQuery] string email,
            [FromQuery] Guid? excludeUserId
        )
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email, excludeUserId);
                return Ok(ApiResponse<object>.Success(new { exists }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 强制重置管理员账号（仅开发环境）
        /// </summary>
        /// <remarks>
        /// 警告：该接口仅用于开发环境！
        /// 会删除所有现有用户并创建新的admin账号。
        /// 生产环境请移除此端点！
        /// </remarks>
        [HttpPost("dev-reset-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> DevResetAdmin()
        {
            try
            {
#if !DEBUG
                return BadRequest(
                    ApiResponse.Error(40000, "This endpoint is only available in development mode")
                );
#endif
                _logger.LogWarning("DEV MODE: Force resetting all users and creating new admin");

                // 直接使用SQL硬删除所有用户（包括软删除的）
                var connectionString = _configuration.GetConnectionString("MySQL");
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var deletedCount = await connection.ExecuteAsync("DELETE FROM `users`");
                    _logger.LogInformation(
                        "Hard deleted {Count} users from database",
                        deletedCount
                    );
                }

                // 创建新的管理员账号
                var createUserDto = new CreateUserDto
                {
                    Username = "admin",
                    Password = "admin",
                    Email = "admin@dawning.com",
                    DisplayName = "Administrator",
                    Role = "admin",
                    IsActive = true,
                };

                var admin = await _userService.CreateAsync(createUserDto, null);

                _logger.LogInformation(
                    "Admin account reset successfully: {Username}",
                    admin.Username
                );

                return Ok(
                    ApiResponse<object>.Success(
                        admin,
                        "Admin account reset successfully. Username: admin, Password: admin"
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting admin account");
                return StatusCode(
                    500,
                    ApiResponse.Error(50000, $"Failed to reset admin account: {ex.Message}")
                );
            }
        }

        /// <summary>
        /// 初始化管理员账号（只能调用一次）
        /// </summary>
        /// <remarks>
        /// 该接口用于系统首次部署时创建初始管理员账号。
        /// 如果系统中已存在任何用户，该接口将返回错误。
        /// 默认账号密码：admin/admin
        /// </remarks>
        [HttpPost("initialize-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> InitializeAdmin()
        {
            try
            {
                _logger.LogInformation("Attempting to initialize admin account");

                // 检查系统中是否已存在任何用户
                var allUsersModel = new UserModel();
                var existingUsers = await _userService.GetPagedListAsync(allUsersModel, 1, 1);

                if (existingUsers.TotalCount > 0)
                {
                    _logger.LogWarning("Admin initialization failed: System already has users");
                    return Conflict(
                        ApiResponse.Error(
                            40900,
                            "System already initialized. Admin account cannot be created again."
                        )
                    );
                }

                // 创建初始管理员账号
                var createUserDto = new CreateUserDto
                {
                    Username = "admin",
                    Password = "admin",
                    Email = "admin@dawning.com",
                    DisplayName = "Administrator",
                    Role = "admin",
                    IsActive = true,
                };

                var admin = await _userService.CreateAsync(createUserDto, null);

                _logger.LogInformation(
                    "Admin account initialized successfully: {Username}",
                    admin.Username
                );

                return Ok(
                    ApiResponse<object>.Success(
                        admin,
                        "Admin account initialized successfully. Please change the default password immediately."
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing admin account");
                return StatusCode(
                    500,
                    ApiResponse.Error(50000, $"Failed to initialize admin account: {ex.Message}")
                );
            }
        }

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        [HttpGet("{id:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            try
            {
                var roles = await _userService.GetUserRolesAsync(id);
                return Ok(ApiResponse<object>.Success(roles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 获取用户详情（含角色）
        /// </summary>
        [HttpGet("{id:guid}/with-roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserWithRoles(Guid id)
        {
            try
            {
                var user = await _userService.GetUserWithRolesAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse.Error(40400, "User not found"));
                }

                return Ok(ApiResponse<object>.Success(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with roles: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        [HttpPost("{id:guid}/roles")]
        [Authorize(Roles = "admin,super_admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRoles(
            Guid id,
            [FromBody] Dawning.Identity.Application.Dtos.Administration.AssignRolesDto dto
        )
        {
            try
            {
                var operatorId = GetCurrentUserId();
                await _userService.AssignRolesAsync(id, dto.RoleIds, operatorId);

                _logger.LogInformation("Roles assigned to user: {UserId}", id);

                return Ok(ApiResponse.Success("Roles assigned successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to assign roles: {Message}", ex.Message);
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user: {UserId}", id);
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
        [Authorize(Roles = "admin,super_admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
        {
            try
            {
                await _userService.RemoveRoleAsync(userId, roleId);

                _logger.LogInformation(
                    "Role removed from user: {UserId}, Role: {RoleId}",
                    userId,
                    roleId
                );

                return Ok(ApiResponse.Success("Role removed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error removing role from user: {UserId}, Role: {RoleId}",
                    userId,
                    roleId
                );
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        #region 辅助方法

        /// <summary>
        /// 获取当前登录用户ID
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }

        #endregion
    }
}
