using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Authentication;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserAuthenticationService userAuthenticationService,
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userAuthenticationService = userAuthenticationService;
            _userService = userService;
            _logger = logger;
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token claims");
                    return Unauthorized(new { message = "Invalid token: user ID not found" });
                }

                // 获取用户信息
                var user = await _userAuthenticationService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(new { message = "User not found" });
                }

                // 转换为响应 DTO
                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Name = user.Username, // 暂时使用 username 作为显示名称
                    Avatar = null, // 暂无头像功能
                    CreatedAt = DateTime.UtcNow, // 暂时返回当前时间
                    IsActive = user.IsActive
                };

                _logger.LogInformation("User info retrieved successfully for user: {UserId}", userId);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user info");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserList(
            [FromQuery] string? username,
            [FromQuery] string? email,
            [FromQuery] string? displayName,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var model = new UserModel
                {
                    Username = username,
                    Email = email,
                    DisplayName = displayName,
                    Role = role,
                    IsActive = isActive
                };

                var result = await _userService.GetPagedListAsync(model, page, pageSize);

                _logger.LogInformation("User list retrieved: page {Page}, total {Total}", page, result.TotalCount);

                return Ok(new
                {
                    code = 0,
                    message = "Success",
                    data = new
                    {
                        list = result.Items,
                        pagination = new
                        {
                            current = result.PageIndex,
                            pageSize = result.PageSize,
                            total = result.TotalCount
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
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
                    return NotFound(new { code = 404, message = "User not found" });
                }

                return Ok(new { code = 0, message = "Success", data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        [HttpPost]
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

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = user.Id },
                    new { code = 0, message = "User created successfully", data = user });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create user: {Message}", ex.Message);
                return BadRequest(new { code = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                dto.Id = id; // 确保ID匹配

                // 获取当前操作者ID
                var operatorId = GetCurrentUserId();

                var user = await _userService.UpdateAsync(dto, operatorId);

                _logger.LogInformation("User updated: {UserId}", id);

                return Ok(new { code = 0, message = "User updated successfully", data = user });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update user: {Message}", ex.Message);
                return BadRequest(new { code = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                // 获取当前操作者ID
                var operatorId = GetCurrentUserId();

                var result = await _userService.DeleteAsync(id, operatorId);

                _logger.LogInformation("User deleted: {UserId}", id);

                return Ok(new { code = 0, message = "User deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to delete user: {Message}", ex.Message);
                return NotFound(new { code = 404, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
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

                return Ok(new { code = 0, message = "Password changed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to change password: {Message}", ex.Message);
                return BadRequest(new { code = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        [HttpGet("check-username")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUsername([FromQuery] string username, [FromQuery] Guid? excludeUserId)
        {
            try
            {
                var exists = await _userService.UsernameExistsAsync(username, excludeUserId);
                return Ok(new { code = 0, data = new { exists } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        [HttpGet("check-email")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] Guid? excludeUserId)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email, excludeUserId);
                return Ok(new { code = 0, data = new { exists } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 获取用户列表（游标分页 - 测试端点）
        /// </summary>
        /// <remarks>
        /// 游标分页适用于大数据集场景，性能优于传统OFFSET分页。
        /// 使用示例：
        /// 1. 第一页：GET /api/user/cursor?pageSize=10
        /// 2. 下一页：GET /api/user/cursor?pageSize=10&cursor={上一页返回的nextCursor}
        /// </remarks>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserListByCursor(
            [FromQuery] int pageSize = 10,
            [FromQuery] long? cursor = null)
        {
            try
            {
                // 调用Service层的游标分页方法
                var result = await _userService.GetPagedListByCursorAsync(pageSize, cursor);

                _logger.LogInformation("User list retrieved by cursor: pageSize {PageSize}, hasNext {HasNext}", 
                    pageSize, result.HasNextPage);

                return Ok(new
                {
                    code = 0,
                    message = "Success",
                    data = new
                    {
                        list = result.Items,
                        pagination = new
                        {
                            pageSize = result.PageSize,
                            hasNextPage = result.HasNextPage,
                            nextCursor = result.NextCursor
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list by cursor");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 测试自定义PagedOptions配置
        /// </summary>
        [HttpGet("custom-config")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserListWithCustomConfig(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? maxPageNumber = null,
            [FromQuery] int? maxCursorPageSize = null)
        {
            try
            {
                var options = new PaginationOptions
                {
                    MaxPageNumber = maxPageNumber ?? 5000,  // 测试自定义最大页数
                    MaxCursorPageSize = maxCursorPageSize ?? 500,  // 测试自定义游标分页限制
                    DefaultPageSize = 20  // 测试自定义默认页大小
                };

                // 通过Service层传递options
                var result = await _userService.GetPagedListWithOptionsAsync(page, pageSize, options);

                _logger.LogInformation("User list retrieved with custom config: page {Page}, total {Total}", 
                    page, result.TotalCount);

                return Ok(new
                {
                    code = 0,
                    message = "Success (custom config applied)",
                    config = new
                    {
                        maxPageNumber = options.MaxPageNumber,
                        maxCursorPageSize = options.MaxCursorPageSize,
                        defaultPageSize = options.DefaultPageSize
                    },
                    data = new
                    {
                        list = result.Items,
                        pagination = new
                        {
                            page = result.PageIndex,
                            pageSize = result.PageSize,
                            total = result.TotalCount
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list with custom config");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        #region 辅助方法

        /// <summary>
        /// 获取当前登录用户ID
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }

        #endregion
    }
}
