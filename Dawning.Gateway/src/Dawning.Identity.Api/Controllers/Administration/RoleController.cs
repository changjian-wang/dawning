using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// 角色管理控制器
    /// </summary>
    [ApiController]
    [Route("api/role")]
    [Authorize(Roles = "admin,super_admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;
        private readonly AuditLogHelper _auditLogHelper;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger, AuditLogHelper auditLogHelper)
        {
            _roleService = roleService;
            _logger = logger;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var role = await _roleService.GetAsync(id);
                if (role == null)
                {
                    return NotFound(new { code = 404, message = "Role not found" });
                }

                return Ok(new { code = 20000, message = "Success", data = role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by ID: {RoleId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 根据名称获取角色
        /// </summary>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                var role = await _roleService.GetByNameAsync(name);
                if (role == null)
                {
                    return NotFound(new { code = 404, message = "Role not found" });
                }

                return Ok(new { code = 20000, message = "Success", data = role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by name: {RoleName}", name);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 获取角色列表（分页）
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList(
            [FromQuery] string? name,
            [FromQuery] string? displayName,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isSystem,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var model = new RoleModel
                {
                    Name = name,
                    DisplayName = displayName,
                    IsActive = isActive,
                    IsSystem = isSystem
                };

                var result = await _roleService.GetPagedListAsync(model, page, pageSize);

                return Ok(new
                {
                    code = 20000,
                    message = "Success",
                    data = new
                    {
                        list = result.Items,
                        pagination = new
                        {
                            total = result.TotalCount,
                            current = result.PageIndex,
                            pageSize = result.PageSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role list");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllAsync();
                return Ok(new { code = 20000, message = "Success", data = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            try
            {
                var operatorId = GetCurrentUserId();
                var role = await _roleService.CreateAsync(dto, operatorId);

                _logger.LogInformation("Role created: {RoleName}", role.Name);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "Create",
                    entityType: "Role",
                    entityId: role.Id,
                    description: $"Created role: {role.Name}",
                    newValues: new { role.Name, role.Description, Permissions = role.Permissions },
                    statusCode: 201);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = role.Id },
                    new { code = 20000, message = "Role created successfully", data = role });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create role: {Message}", ex.Message);
                return BadRequest(new { code = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                dto.Id = id;
                var operatorId = GetCurrentUserId();

                // 获取更新前的角色信息
                var oldRole = await _roleService.GetAsync(id);

                var role = await _roleService.UpdateAsync(dto, operatorId);

                _logger.LogInformation("Role updated: {RoleId}", id);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "Update",
                    entityType: "Role",
                    entityId: id,
                    description: $"Updated role: {role.Name}",
                    oldValues: oldRole != null ? new { oldRole.Name, oldRole.Description, Permissions = oldRole.Permissions } : null,
                    newValues: new { role.Name, role.Description, Permissions = role.Permissions });

                return Ok(new { code = 20000, message = "Role updated successfully", data = role });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update role: {Message}", ex.Message);
                return BadRequest(new { code = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var operatorId = GetCurrentUserId();

                // 获取删除前的角色信息
                var role = await _roleService.GetAsync(id);

                await _roleService.DeleteAsync(id, operatorId);

                _logger.LogInformation("Role deleted: {RoleId}", id);

                // 记录审计日志
                await _auditLogHelper.LogAsync(
                    action: "Delete",
                    entityType: "Role",
                    entityId: id,
                    description: $"Deleted role: {role?.Name}",
                    oldValues: role != null ? new { role.Name, role.Description, Permissions = role.Permissions } : null);

                return Ok(new { code = 20000, message = "Role deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to delete role: {Message}", ex.Message);
                return BadRequest(new { code = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", id);
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        [HttpGet("check-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckName([FromQuery] string name, [FromQuery] Guid? excludeRoleId)
        {
            try
            {
                var exists = await _roleService.NameExistsAsync(name, excludeRoleId);
                return Ok(new { code = 20000, data = new { exists } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role name");
                return StatusCode(500, new { code = 500, message = "Internal server error" });
            }
        }

        /// <summary>
        /// 获取当前登录用户ID
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
