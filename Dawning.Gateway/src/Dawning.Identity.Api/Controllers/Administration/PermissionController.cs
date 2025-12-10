using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// 权限管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// 获取单个权限
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var permission = await _permissionService.GetAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = $"权限 ID '{id}' 不存在" });
            }

            return Ok(new { data = permission });
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
                return NotFound(new { message = $"权限代码 '{code}' 不存在" });
            }

            return Ok(new { data = permission });
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
            [FromQuery] int current = 1,
            [FromQuery] int pageSize = 20)
        {
            var model = new PermissionModel
            {
                Code = code,
                Name = name,
                Resource = resource,
                Action = action,
                Category = category,
                IsActive = isActive
            };

            var result = await _permissionService.GetPagedListAsync(model, current, pageSize);

            return Ok(new
            {
                data = new
                {
                    pagination = new
                    {
                        total = result.TotalCount,
                        current = result.PageIndex,
                        pageSize = result.PageSize
                    },
                    list = result.Items
                }
            });
        }

        /// <summary>
        /// 获取所有权限
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(new { data = permissions });
        }

        /// <summary>
        /// 获取分组权限（按资源分组）
        /// </summary>
        [HttpGet("grouped")]
        public async Task<IActionResult> GetGroupedPermissionsAsync()
        {
            var groups = await _permissionService.GetGroupedPermissionsAsync();
            return Ok(new { data = groups });
        }

        /// <summary>
        /// 获取角色的权限
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleIdAsync(Guid roleId)
        {
            var permissions = await _permissionService.GetByRoleIdAsync(roleId);
            return Ok(new { data = permissions });
        }

        /// <summary>
        /// 创建权限
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePermissionDto dto)
        {
            try
            {
                var operatorId = GetOperatorId();
                var permission = await _permissionService.CreateAsync(dto, operatorId);
                return Ok(new { data = permission, message = "权限创建成功" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdatePermissionDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID 不匹配" });
            }

            try
            {
                var operatorId = GetOperatorId();
                var permission = await _permissionService.UpdateAsync(dto, operatorId);
                return Ok(new { data = permission, message = "权限更新成功" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                await _permissionService.DeleteAsync(id);
                return Ok(new { message = "权限删除成功" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 为角色分配权限
        /// </summary>
        [HttpPost("role/{roleId}/assign")]
        public async Task<IActionResult> AssignPermissionsToRoleAsync(
            Guid roleId,
            [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var operatorId = GetOperatorId();
                await _permissionService.AssignPermissionsToRoleAsync(roleId, permissionIds, operatorId);
                return Ok(new { message = "权限分配成功" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 从角色移除权限
        /// </summary>
        [HttpDelete("role/{roleId}/remove")]
        public async Task<IActionResult> RemovePermissionsFromRoleAsync(
            Guid roleId,
            [FromBody] List<Guid> permissionIds)
        {
            try
            {
                await _permissionService.RemovePermissionsFromRoleAsync(roleId, permissionIds);
                return Ok(new { message = "权限移除成功" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 检查角色是否拥有权限
        /// </summary>
        [HttpGet("role/{roleId}/has/{permissionCode}")]
        public async Task<IActionResult> HasPermissionAsync(Guid roleId, string permissionCode)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(roleId, permissionCode);
            return Ok(new { data = hasPermission });
        }

        /// <summary>
        /// 获取用户的所有权限代码
        /// </summary>
        [HttpGet("user/{userId}/codes")]
        public async Task<IActionResult> GetUserPermissionCodesAsync(Guid userId)
        {
            var permissionCodes = await _permissionService.GetUserPermissionCodesAsync(userId);
            return Ok(new { data = permissionCodes });
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
