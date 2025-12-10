using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Services.Administration
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PermissionDto?> GetAsync(Guid id)
        {
            var permission = await _unitOfWork.Permission.GetAsync(id);
            return permission?.ToDto();
        }

        public async Task<PermissionDto?> GetByCodeAsync(string code)
        {
            var permission = await _unitOfWork.Permission.GetByCodeAsync(code);
            return permission?.ToDto();
        }

        public async Task<PagedData<PermissionDto>> GetPagedListAsync(PermissionModel model, int page, int pageSize)
        {
            var pagedData = await _unitOfWork.Permission.GetPagedListAsync(model, page, pageSize);
            
            return new PagedData<PermissionDto>
            {
                Items = pagedData.Items.Select(p => p.ToDto()).ToList(),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize
            };
        }

        public async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            var permissions = await _unitOfWork.Permission.GetAllAsync();
            return permissions.Select(p => p.ToDto());
        }

        public async Task<IEnumerable<PermissionDto>> GetByRoleIdAsync(Guid roleId)
        {
            var permissions = await _unitOfWork.Permission.GetByRoleIdAsync(roleId);
            return permissions.Select(p => p.ToDto());
        }

        public async Task<IEnumerable<PermissionGroupDto>> GetGroupedPermissionsAsync()
        {
            var permissions = await _unitOfWork.Permission.GetAllAsync();
            
            var grouped = permissions
                .GroupBy(p => p.Resource)
                .Select(g => new PermissionGroupDto
                {
                    Resource = g.Key,
                    ResourceName = g.Key,
                    Permissions = g.Select(p => p.ToDto()).OrderBy(p => p.DisplayOrder).ToList()
                })
                .OrderBy(g => g.Resource)
                .ToList();

            return grouped;
        }

        public async Task<PermissionDto> CreateAsync(CreatePermissionDto dto, Guid? operatorId = null)
        {
            // 验证代码唯一性
            var exists = await _unitOfWork.Permission.CodeExistsAsync(dto.Code);
            if (exists)
            {
                throw new InvalidOperationException($"权限代码 '{dto.Code}' 已存在");
            }

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Resource = dto.Resource,
                Action = dto.Action,
                Category = dto.Category,
                IsSystem = false,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId
            };

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.Permission.InsertAsync(permission);
                
                // 记录审计日志
                await _unitOfWork.AuditLog.InsertAsync(new Domain.Aggregates.Administration.AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = "Create",
                    EntityType = "Permission",
                    EntityId = permission.Id,
                    Description = $"创建权限: {permission.Name} ({permission.Code})",
                    IpAddress = null,
                    UserAgent = null,
                    StatusCode = 200,
                    UserId = operatorId,
                    Username = null,
                    CreatedAt = DateTime.UtcNow
                });

                _unitOfWork.Commit();
                return permission.ToDto();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<PermissionDto> UpdateAsync(UpdatePermissionDto dto, Guid? operatorId = null)
        {
            var permission = await _unitOfWork.Permission.GetAsync(dto.Id);
            if (permission == null)
            {
                throw new InvalidOperationException($"权限 ID '{dto.Id}' 不存在");
            }

            // 系统权限不允许修改代码和关键属性
            if (permission.IsSystem)
            {
                throw new InvalidOperationException("系统权限不允许修改");
            }

            var oldValues = $"{permission.Name}|{permission.Description}";

            permission.Name = dto.Name;
            permission.Description = dto.Description;
            permission.IsActive = dto.IsActive;
            permission.DisplayOrder = dto.DisplayOrder;
            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedBy = operatorId;

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.Permission.UpdateAsync(permission);
                
                // 记录审计日志
                await _unitOfWork.AuditLog.InsertAsync(new Domain.Aggregates.Administration.AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = "Update",
                    EntityType = "Permission",
                    EntityId = permission.Id,
                    Description = $"更新权限: {oldValues} -> {permission.Name}|{permission.Description}",
                    IpAddress = null,
                    UserAgent = null,
                    StatusCode = 200,
                    UserId = operatorId,
                    Username = null,
                    CreatedAt = DateTime.UtcNow
                });

                _unitOfWork.Commit();
                return permission.ToDto();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var permission = await _unitOfWork.Permission.GetAsync(id);
            if (permission == null)
            {
                throw new InvalidOperationException($"权限 ID '{id}' 不存在");
            }

            // 系统权限不允许删除
            if (permission.IsSystem)
            {
                throw new InvalidOperationException("系统权限不允许删除");
            }

            // 检查是否有角色使用此权限
            var rolePermissions = await _unitOfWork.RolePermission.GetByPermissionIdAsync(id);
            if (rolePermissions.Any())
            {
                throw new InvalidOperationException($"权限 '{permission.Name}' 正在被 {rolePermissions.Count()} 个角色使用,无法删除");
            }

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.Permission.DeleteAsync(permission);
                
                // 记录审计日志
                await _unitOfWork.AuditLog.InsertAsync(new Domain.Aggregates.Administration.AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = "Delete",
                    EntityType = "Permission",
                    EntityId = id,
                    Description = $"删除权限: {permission.Name} ({permission.Code})",
                    IpAddress = null,
                    UserAgent = null,
                    StatusCode = 200,
                    UserId = null,
                    Username = null,
                    CreatedAt = DateTime.UtcNow
                });

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid? operatorId = null)
        {
            var role = await _unitOfWork.Role.GetAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"角色 ID '{roleId}' 不存在");
            }

            var permissions = await _unitOfWork.Permission.GetByIdsAsync(permissionIds);
            var existingIds = permissions.Select(p => p.Id).ToList();
            var missingIds = permissionIds.Except(existingIds).ToList();

            if (missingIds.Any())
            {
                throw new InvalidOperationException($"权限 ID {string.Join(", ", missingIds)} 不存在");
            }

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.RolePermission.AddRolePermissionsAsync(roleId, permissionIds, operatorId);
                
                // 记录审计日志
                await _unitOfWork.AuditLog.InsertAsync(new Domain.Aggregates.Administration.AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = "AssignPermissions",
                    EntityType = "Role",
                    EntityId = roleId,
                    Description = $"为角色 '{role.Name}' 分配 {permissionIds.Count()} 个权限",
                    IpAddress = null,
                    UserAgent = null,
                    StatusCode = 200,
                    UserId = operatorId,
                    Username = null,
                    CreatedAt = DateTime.UtcNow
                });

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds)
        {
            var role = await _unitOfWork.Role.GetAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"角色 ID '{roleId}' 不存在");
            }

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.RolePermission.RemoveRolePermissionsAsync(roleId, permissionIds);
                
                // 记录审计日志
                await _unitOfWork.AuditLog.InsertAsync(new Domain.Aggregates.Administration.AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = "RemovePermissions",
                    EntityType = "Role",
                    EntityId = roleId,
                    Description = $"从角色 '{role.Name}' 移除 {permissionIds.Count()} 个权限",
                    IpAddress = null,
                    UserAgent = null,
                    StatusCode = 200,
                    UserId = null,
                    Username = null,
                    CreatedAt = DateTime.UtcNow
                });

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionCode)
        {
            return await _unitOfWork.RolePermission.HasPermissionAsync(roleId, permissionCode);
        }

        public async Task<IEnumerable<string>> GetUserPermissionCodesAsync(Guid userId)
        {
            // 获取用户的所有角色
            var userRoles = await _unitOfWork.UserRole.GetUserRolesAsync(userId);
            
            if (!userRoles.Any())
            {
                return new List<string>();
            }

            // 获取所有角色的权限(去重)
            var allPermissions = new List<Permission>();
            foreach (var role in userRoles)
            {
                var rolePermissions = await _unitOfWork.Permission.GetByRoleIdAsync(role.Id);
                allPermissions.AddRange(rolePermissions);
            }

            // 去重并返回权限代码
            return allPermissions
                .Where(p => p.IsActive)
                .Select(p => p.Code)
                .Distinct()
                .ToList();
        }
    }

    // 扩展方法
    public static class PermissionExtensions
    {
        public static PermissionDto ToDto(this Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Code = permission.Code,
                Name = permission.Name,
                Description = permission.Description,
                Resource = permission.Resource,
                Action = permission.Action,
                Category = permission.Category,
                IsSystem = permission.IsSystem,
                IsActive = permission.IsActive,
                DisplayOrder = permission.DisplayOrder,
                CreatedAt = permission.CreatedAt,
                UpdatedAt = permission.UpdatedAt
            };
        }
    }
}
