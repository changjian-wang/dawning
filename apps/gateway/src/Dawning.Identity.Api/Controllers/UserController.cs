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
    /// User management controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly AuditLogHelper _auditLogHelper;

        public UserController(
            IUserAuthenticationService userAuthenticationService,
            IUserService userService,
            IRoleService roleService,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<UserController> logger,
            AuditLogHelper auditLogHelper
        )
        {
            _userAuthenticationService = userAuthenticationService;
            _userService = userService;
            _roleService = roleService;
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// Get current logged-in user information
        /// </summary>
        /// <returns>User information</returns>
        [HttpGet("info")]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            try
            {
                // Get user ID from JWT Claims
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

                // Get user information
                var user = await _userAuthenticationService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(ApiResponse.Error(40400, "User not found"));
                }

                // Convert to response DTO
                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username!,
                    Email = user.Email!,
                    Roles = user.Roles,
                    Name = user.Username!, // Temporarily use username as display name
                    Avatar = null, // Avatar feature not available yet
                    CreatedAt = DateTime.UtcNow, // Temporarily return current time
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
        /// Get user list (paginated)
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
        /// Get user list (cursor pagination)
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
        /// Get user details by ID
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
        /// Create user
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // Get current operator ID
                var operatorId = GetCurrentUserId();

                var user = await _userService.CreateAsync(dto, operatorId);

                _logger.LogInformation("User created: {Username}", user.Username);

                // Record audit log
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
        /// Update user
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                dto.Id = id; // Ensure ID matches

                // Get current operator ID
                var operatorId = GetCurrentUserId();

                // Get user info before update
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

                // Record audit log
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
        /// Delete user
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                // Get current operator ID
                var operatorId = GetCurrentUserId();

                // Get deleted user info
                var user = await _userRepository.GetAsync(id);
                var userInfo = user != null ? new { user.Username, user.Email } : null;

                var result = await _userService.DeleteAsync(id, operatorId);

                _logger.LogInformation("User deleted: {UserId}", id);

                // Record audit log
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
        /// Batch delete users
        /// </summary>
        [HttpDelete("batch")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BatchDeleteUsers([FromBody] BatchUserIdsRequest request)
        {
            try
            {
                if (request.Ids == null || !request.Ids.Any())
                {
                    return BadRequest(ApiResponse.Error(40000, "No user IDs provided"));
                }

                var operatorId = GetCurrentUserId();
                var successCount = 0;
                var failedIds = new List<Guid>();

                foreach (var id in request.Ids)
                {
                    try
                    {
                        await _userService.DeleteAsync(id, operatorId);
                        successCount++;
                    }
                    catch
                    {
                        failedIds.Add(id);
                    }
                }

                _logger.LogInformation(
                    "Batch delete users: success={SuccessCount}, failed={FailedCount}",
                    successCount,
                    failedIds.Count
                );

                // Record audit log
                await _auditLogHelper.LogAsync(
                    action: "BatchDelete",
                    entityType: "User",
                    entityId: null,
                    description: $"Batch deleted {successCount} users, {failedIds.Count} failed"
                );

                return Ok(
                    ApiResponse<object>.Success(
                        new
                        {
                            successCount,
                            failedCount = failedIds.Count,
                            failedIds,
                        },
                        $"Successfully deleted {successCount} users"
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch deleting users");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// Batch enable/disable users
        /// </summary>
        [HttpPost("batch/status")]
        [Authorize(Roles = "admin,super_admin,user_manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BatchUpdateUserStatus(
            [FromBody] BatchUpdateStatusRequest request
        )
        {
            try
            {
                if (request.Ids == null || !request.Ids.Any())
                {
                    return BadRequest(ApiResponse.Error(40000, "No user IDs provided"));
                }

                var operatorId = GetCurrentUserId();
                var successCount = 0;
                var failedIds = new List<Guid>();

                foreach (var id in request.Ids)
                {
                    try
                    {
                        var user = await _userRepository.GetAsync(id);
                        if (user != null)
                        {
                            // Protect system users, prevent disabling
                            if (!request.IsActive && user.IsSystem)
                            {
                                failedIds.Add(id);
                                continue;
                            }

                            user.IsActive = request.IsActive;
                            user.UpdatedAt = DateTime.UtcNow;
                            user.UpdatedBy = operatorId;
                            await _userRepository.UpdateAsync(user);
                            successCount++;
                        }
                        else
                        {
                            failedIds.Add(id);
                        }
                    }
                    catch
                    {
                        failedIds.Add(id);
                    }
                }

                _logger.LogInformation(
                    "Batch update user status: isActive={IsActive}, success={SuccessCount}, failed={FailedCount}",
                    request.IsActive,
                    successCount,
                    failedIds.Count
                );

                // Record audit log
                await _auditLogHelper.LogAsync(
                    action: request.IsActive ? "BatchEnable" : "BatchDisable",
                    entityType: "User",
                    entityId: null,
                    description: $"Batch {(request.IsActive ? "enabled" : "disabled")} {successCount} users, {failedIds.Count} failed"
                );

                return Ok(
                    ApiResponse<object>.Success(
                        new
                        {
                            successCount,
                            failedCount = failedIds.Count,
                            failedIds,
                        },
                        $"Successfully {(request.IsActive ? "enabled" : "disabled")} {successCount} users"
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch updating user status");
                return StatusCode(500, ApiResponse.Error(50000, "Internal server error"));
            }
        }

        /// <summary>
        /// Change password
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

                // Record audit log
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
        /// Reset password (admin function)
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

                // Get user info
                var user = await _userRepository.GetAsync(id);

                // Record audit log
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
        /// Check if username exists
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
        /// Check if email exists
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
        /// Force reset admin account (development environment only)
        /// </summary>
        /// <remarks>
        /// Warning: This endpoint is only for development environment!
        /// It will delete all existing users and create a new admin account.
        /// Please remove this endpoint in production!
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

                // Use SQL to hard delete all users (including soft-deleted ones)
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

                // Create new admin account
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
        /// Initialize super admin account (can only be called once)
        /// </summary>
        /// <remarks>
        /// This endpoint is used to create the initial super admin account during first system deployment.
        /// If any users already exist in the system, this endpoint will return an error.
        /// Default credentials: admin/admin, role: super_admin (Super Administrator)
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

                // Check if any users already exist in the system
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

                // Create initial super admin account
                var createUserDto = new CreateUserDto
                {
                    Username = "admin",
                    Password = "Admin@123",
                    Email = "admin@dawning.com",
                    DisplayName = "Super Administrator",
                    Role = "super_admin",
                    IsActive = true,
                    IsSystem = true, // Mark as system user, cannot be deleted/disabled
                };

                var admin = await _userService.CreateAsync(createUserDto, null);

                // Find and assign super_admin role
                var superAdminRole = await _roleService.GetByNameAsync("super_admin");
                if (superAdminRole?.Id != null)
                {
                    await _userService.AssignRolesAsync(
                        admin.Id,
                        new List<Guid> { superAdminRole.Id.Value },
                        null
                    );
                    _logger.LogInformation(
                        "super_admin role assigned to admin user: {UserId}",
                        admin.Id
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "super_admin role not found, admin user created without role assignment"
                    );
                }

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
        /// Fix admin user role assignment (temporary endpoint)
        /// </summary>
        /// <remarks>
        /// Assigns super_admin role to admin user.
        /// This is a one-time fix endpoint for admin users created earlier without role assignment.
        /// </remarks>
        [HttpPost("fix-admin-roles")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FixAdminRoles()
        {
            try
            {
                // Find admin user
                var admin = await _userService.GetByUsernameAsync("admin");
                if (admin == null)
                {
                    return NotFound(ApiResponse.Error(40400, "Admin user not found"));
                }

                // Find super_admin role
                var superAdminRole = await _roleService.GetByNameAsync("super_admin");
                if (superAdminRole?.Id == null)
                {
                    return NotFound(ApiResponse.Error(40400, "super_admin role not found"));
                }

                // Check if already assigned
                var existingRoles = await _userService.GetUserRolesAsync(admin.Id);
                if (existingRoles.Any(r => r.Name == "super_admin"))
                {
                    return Ok(
                        ApiResponse<object>.Success(
                            new
                            {
                                adminId = admin.Id,
                                message = "Admin already has super_admin role",
                            }
                        )
                    );
                }

                // Assign role
                await _userService.AssignRolesAsync(
                    admin.Id,
                    new List<Guid> { superAdminRole.Id.Value },
                    null
                );

                _logger.LogInformation(
                    "super_admin role assigned to existing admin user: {UserId}",
                    admin.Id
                );

                return Ok(
                    ApiResponse<object>.Success(
                        new { adminId = admin.Id, roleId = superAdminRole.Id },
                        "super_admin role assigned to admin user successfully"
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing admin roles");
                return StatusCode(
                    500,
                    ApiResponse.Error(50000, $"Failed to fix admin roles: {ex.Message}")
                );
            }
        }

        /// <summary>
        /// Get user role list
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
        /// Get user details (with roles)
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
        /// Assign roles to user
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
        /// Remove user role
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

        #region Helper Methods

        /// <summary>
        /// Get current logged-in user ID
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }

        #endregion
    }

    #region Request Models

    /// <summary>
    /// Batch user IDs request
    /// </summary>
    public class BatchUserIdsRequest
    {
        /// <summary>
        /// User ID list
        /// </summary>
        public List<Guid> Ids { get; set; } = new();
    }

    /// <summary>
    /// Batch update status request
    /// </summary>
    public class BatchUpdateStatusRequest
    {
        /// <summary>
        /// User ID list
        /// </summary>
        public List<Guid> Ids { get; set; } = new();

        /// <summary>
        /// Whether to enable
        /// </summary>
        public bool IsActive { get; set; }
    }

    #endregion
}
