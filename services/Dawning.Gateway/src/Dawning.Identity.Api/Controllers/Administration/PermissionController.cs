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
    /// 权限管理控制器
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
        /// 获取单个权限
        /// </summary>
        [HttpGet("{id:guid}")]
        [RequirePermission("permission.read")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var permission = await _permissionService.GetAsync(id);
            if (permission == null)
            {
                return NotFound(ApiResponse.Error(40400, $"权限 ID '{id}' 不存在"));
            }

            return Ok(ApiResponse<object>.Success(permission));
        }

        /// <summary>
        /// 根据代码获取权限
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var permission = await _permissionService.GetByCodeAsync(code);
            if (permission == null)
            {
                return NotFound(ApiResponse.Error(40400, $"权限代码 '{code}' 不存在"));
            }

            return Ok(ApiResponse<object>.Success(permission));
        }

        /// <summary>
        /// 获取权限分页列表
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
        /// 获取所有权限
        /// </summary>
        [HttpGet("all")]
        [RequirePermission("permission.read")]
        public async Task<IActionResult> GetAllAsync()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(ApiResponse<object>.Success(permissions));
        }

        /// <summary>
        /// 获取分组权限（按资源分组）
        /// </summary>
        [HttpGet("grouped")]
        public async Task<IActionResult> GetGroupedPermissionsAsync()
        {
            var groups = await _permissionService.GetGroupedPermissionsAsync();
            return Ok(ApiResponse<object>.Success(groups));
        }

        /// <summary>
        /// 获取所有资源类型
        /// </summary>
        [HttpGet("resources")]
        public async Task<IActionResult> GetResourceTypesAsync()
        {
            var resources = await _permissionService.GetResourceTypesAsync();
            return Ok(ApiResponse<object>.Success(resources));
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            var categories = await _permissionService.GetCategoriesAsync();
            return Ok(ApiResponse<object>.Success(categories));
        }

        /// <summary>
        /// 获取角色的权限
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleIdAsync(Guid roleId)
        {
            var permissions = await _permissionService.GetByRoleIdAsync(roleId);
            return Ok(ApiResponse<object>.Success(permissions));
        }

        /// <summary>
        /// 创建权限
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
                    $"创建权限: {dto.Code}",
                    null,
                    dto
                );

                return Ok(ApiResponse<object>.Success(permission, "权限创建成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        [HttpPut("{id:guid}")]
        [RequirePermission("permission.update")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdatePermissionDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.Error(40000, "ID 不匹配"));
            }

            try
            {
                var operatorId = GetOperatorId();
                var permission = await _permissionService.UpdateAsync(dto, operatorId);

                await _auditLogHelper.LogAsync(
                    "UpdatePermission",
                    "Permission",
                    id,
                    $"更新权限: {dto.Name}",
                    null,
                    dto
                );

                return Ok(ApiResponse<object>.Success(permission, "权限更新成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// 删除权限
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
                    $"删除权限: {id}"
                );

                return Ok(ApiResponse.Success("权限删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// 为角色分配权限
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
                    $"为角色分配权限，权限数量: {permissionIds.Count}",
                    null,
                    new { PermissionIds = permissionIds }
                );

                return Ok(ApiResponse.Success("权限分配成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// 从角色移除权限
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
                    $"从角色移除权限，权限数量: {permissionIds.Count}",
                    new { PermissionIds = permissionIds },
                    null
                );

                return Ok(ApiResponse.Success("权限移除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(40000, ex.Message));
            }
        }

        /// <summary>
        /// 检查角色是否拥有权限
        /// </summary>
        [HttpGet("role/{roleId}/has/{permissionCode}")]
        public async Task<IActionResult> HasPermissionAsync(Guid roleId, string permissionCode)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(roleId, permissionCode);
            return Ok(ApiResponse<object>.Success(hasPermission));
        }

        /// <summary>
        /// 获取用户的所有权限代码
        /// </summary>
        [HttpGet("user/{userId}/codes")]
        public async Task<IActionResult> GetUserPermissionCodesAsync(Guid userId)
        {
            var permissionCodes = await _permissionService.GetUserPermissionCodesAsync(userId);
            return Ok(ApiResponse<object>.Success(permissionCodes));
        }

        /// <summary>
        /// 获取当前用户ID
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
