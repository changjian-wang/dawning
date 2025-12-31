using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Api.Helpers;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Api.Security;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// Permission management controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly AuditLogHelper _auditLogHelper;

        public PermissionController(
            IPermissionService permissionService,
            AuditLogHelper auditLogHelper
        )
        {
            _permissionService = permissionService;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// Get single permission
        /// </summary>
        [HttpGet("{id:guid}")]
        [RequirePermission("permission.read")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var permission = await _permissionService.GetAsync(id);
            if (permission == null)
            {
                return NotFound(ApiResponse.Error(40400, $"Permission ID '{id}' does not exist"));
            }

            return Ok(ApiResponse<object>.Success(permission));
        }

        /// <summary>
        /// Get permission by code
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var permission = await _permissionService.GetByCodeAsync(code);
            if (permission == null)
            {
                return NotFound(ApiResponse.Error(40400, $"Permission code '{code}' does not exist"));
            }

            return Ok(ApiResponse<object>.Success(permission));
        }

        /// <summary>
        /// Get permission paginated list
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPagedListAsync(
            [FromQuery] string? code,
            [FromQuery] string? name,
            [FromQuery] string? resource,
            [FromQuery] string? action,
            [FromQuery] string? category,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var model = new PermissionModel
            {
                Code = code,
                Name = name,
                Resource = resource,
                Action = action,
                Category = category,
                IsActive = isActive,
            };

            var result = await _permissionService.GetPagedListAsync(model, page, pageSize);

            return Ok(ApiResponse<object>.Success(result));
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        [HttpGet("all")]
        [RequirePermission("permission.read")]
        public async Task<IActionResult> GetAllAsync()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(ApiResponse<object>.Success(permissions));
        }

        /// <summary>
        /// Get grouped permissions (grouped by resource)
        /// </summary>
        [HttpGet("grouped")]
        public async Task<IActionResult> GetGroupedPermissionsAsync()
        {
            var groups = await _permissionService.GetGroupedPermissionsAsync();
            return Ok(ApiResponse<object>.Success(groups));
        }

        /// <summary>
        /// Get all resource types
        /// </summary>
        [HttpGet("resources")]
        public async Task<IActionResult> GetResourceTypesAsync()
        {
            var resources = await _permissionService.GetResourceTypesAsync();
            return Ok(ApiResponse<object>.Success(resources));
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            var categories = await _permissionService.GetCategoriesAsync();
            return Ok(ApiResponse<object>.Success(categories));
        }

        /// <summary>
        /// Get permissions for a role
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleIdAsync(Guid roleId)
        {
            var permissions = await _permissionService.GetByRoleIdAsync(roleId);
            return Ok(ApiResponse<object>.Success(permissions));
        }

        /// <summary>
        /// Create permission
        /// </summary>
        [HttpPost]
        [RequirePermission("permission.create")]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePermissionDto dto)
        {
            try
            {
                var operatorId = GetOperatorId();
                var permission = await _permissionService.CreateAsync(dto, operatorId);

                await _auditLogHelper.LogAsync(
                    "CreatePermission",
                    "Permission",
                    permission.Id,
                    $"Created permission: {dto.Code}",
                    null,
                    dto
                );

                return Ok(ApiResponse<object>.Success(permission, "Permission created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// Update permission
        /// </summary>
        [HttpPut("{id:guid}")]
        [RequirePermission("permission.update")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdatePermissionDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.Error(40000, "ID mismatch"));
            }

            try
            {
                var operatorId = GetOperatorId();
                var permission = await _permissionService.UpdateAsync(dto, operatorId);

                await _auditLogHelper.LogAsync(
                    "UpdatePermission",
                    "Permission",
                    id,
                    $"Updated permission: {dto.Name}",
                    null,
                    dto
                );

                return Ok(ApiResponse<object>.Success(permission, "Permission updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// Delete permission
        /// </summary>
        [HttpDelete("{id:guid}")]
        [RequirePermission("permission.delete")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                await _permissionService.DeleteAsync(id);

                await _auditLogHelper.LogAsync(
                    "DeletePermission",
                    "Permission",
                    id,
                    $"Deleted permission: {id}"
                );

                return Ok(ApiResponse.Success("Permission deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// Assign permissions to role
        /// </summary>
        [HttpPost("role/{roleId}/assign")]
        [RequirePermission("role.permission.assign")]
        public async Task<IActionResult> AssignPermissionsToRoleAsync(
            Guid roleId,
            [FromBody] List<Guid> permissionIds
        )
        {
            try
            {
                var operatorId = GetOperatorId();
                await _permissionService.AssignPermissionsToRoleAsync(
                    roleId,
                    permissionIds,
                    operatorId
                );

                await _auditLogHelper.LogAsync(
                    "AssignPermissions",
                    "Role",
                    roleId,
                    $"Assigned permissions to role, count: {permissionIds.Count}",
                    null,
                    new { PermissionIds = permissionIds }
                );

                return Ok(ApiResponse.Success("Permissions assigned successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// Remove permissions from role
        /// </summary>
        [HttpDelete("role/{roleId}/remove")]
        public async Task<IActionResult> RemovePermissionsFromRoleAsync(
            Guid roleId,
            [FromBody] List<Guid> permissionIds
        )
        {
            try
            {
                await _permissionService.RemovePermissionsFromRoleAsync(roleId, permissionIds);

                await _auditLogHelper.LogAsync(
                    "RemovePermissions",
                    "Role",
                    roleId,
                    $"Removed permissions from role, count: {permissionIds.Count}",
                    new { PermissionIds = permissionIds },
                    null
                );

                return Ok(ApiResponse.Success("Permissions removed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// Check if role has permission
        /// </summary>
        [HttpGet("role/{roleId}/has/{permissionCode}")]
        public async Task<IActionResult> HasPermissionAsync(Guid roleId, string permissionCode)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(roleId, permissionCode);
            return Ok(ApiResponse<object>.Success(hasPermission));
        }

        /// <summary>
        /// Get all permission codes for a user
        /// </summary>
        [HttpGet("user/{userId}/codes")]
        public async Task<IActionResult> GetUserPermissionCodesAsync(Guid userId)
        {
            var permissionCodes = await _permissionService.GetUserPermissionCodesAsync(userId);
            return Ok(ApiResponse<object>.Success(permissionCodes));
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        private Guid? GetOperatorId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
