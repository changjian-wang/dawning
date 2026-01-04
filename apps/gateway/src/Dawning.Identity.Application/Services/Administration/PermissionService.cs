using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

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

        public async Task<PagedData<PermissionDto>> GetPagedListAsync(
            PermissionModel model,
            int page,
            int pageSize
        )
        {
            var pagedData = await _unitOfWork.Permission.GetPagedListAsync(model, page, pageSize);

            return new PagedData<PermissionDto>
            {
                Items = pagedData.Items.Select(p => p.ToDto()).ToList(),
                TotalCount = pagedData.TotalCount,
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
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
                    Permissions = g.Select(p => p.ToDto()).OrderBy(p => p.DisplayOrder).ToList(),
                })
                .OrderBy(g => g.Resource)
                .ToList();

            return grouped;
        }

        public async Task<IEnumerable<string>> GetResourceTypesAsync()
        {
            var permissions = await _unitOfWork.Permission.GetAllAsync();
            return permissions
                .Select(p => p.Resource)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .OrderBy(r => r)
                .Select(r => r!)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var permissions = await _unitOfWork.Permission.GetAllAsync();
            return permissions
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .Select(c => c!)
                .ToList();
        }

        public async Task<PermissionDto> CreateAsync(
            CreatePermissionDto dto,
            Guid? operatorId = null
        )
        {
            // Validate code uniqueness
            var exists = await _unitOfWork.Permission.CodeExistsAsync(dto.Code);
            if (exists)
            {
                throw new InvalidOperationException($"Permission code '{dto.Code}' already exists");
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
                CreatedBy = operatorId,
            };

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.Permission.InsertAsync(permission);

                // Record audit log
                await _unitOfWork.AuditLog.InsertAsync(
                    new Domain.Aggregates.Administration.AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "Create",
                        EntityType = "Permission",
                        EntityId = permission.Id,
                        Description = $"Created permission: {permission.Name} ({permission.Code})",
                        IpAddress = null,
                        UserAgent = null,
                        StatusCode = 200,
                        UserId = operatorId,
                        Username = null,
                        CreatedAt = DateTime.UtcNow,
                    }
                );

                _unitOfWork.Commit();
                return permission.ToDto();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<PermissionDto> UpdateAsync(
            UpdatePermissionDto dto,
            Guid? operatorId = null
        )
        {
            var permission = await _unitOfWork.Permission.GetAsync(dto.Id);
            if (permission == null)
            {
                throw new InvalidOperationException($"Permission ID '{dto.Id}' not found");
            }

            // System permissions cannot have key properties modified
            if (permission.IsSystem)
            {
                throw new InvalidOperationException("System permissions cannot be modified");
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

                // Record audit log
                await _unitOfWork.AuditLog.InsertAsync(
                    new Domain.Aggregates.Administration.AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "Update",
                        EntityType = "Permission",
                        EntityId = permission.Id,
                        Description =
                            $"Updated permission: {oldValues} -> {permission.Name}|{permission.Description}",
                        IpAddress = null,
                        UserAgent = null,
                        StatusCode = 200,
                        UserId = operatorId,
                        Username = null,
                        CreatedAt = DateTime.UtcNow,
                    }
                );

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
                throw new InvalidOperationException($"Permission ID '{id}' not found");
            }

            // System permissions cannot be deleted
            if (permission.IsSystem)
            {
                throw new InvalidOperationException("System permissions cannot be deleted");
            }

            // Check if any role is using this permission
            var rolePermissions = await _unitOfWork.RolePermission.GetByPermissionIdAsync(id);
            if (rolePermissions.Any())
            {
                throw new InvalidOperationException(
                    $"Permission '{permission.Name}' is being used by {rolePermissions.Count()} role(s), cannot be deleted"
                );
            }

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.Permission.DeleteAsync(permission);

                // Record audit log
                await _unitOfWork.AuditLog.InsertAsync(
                    new Domain.Aggregates.Administration.AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "Delete",
                        EntityType = "Permission",
                        EntityId = id,
                        Description = $"Deleted permission: {permission.Name} ({permission.Code})",
                        IpAddress = null,
                        UserAgent = null,
                        StatusCode = 200,
                        UserId = null,
                        Username = null,
                        CreatedAt = DateTime.UtcNow,
                    }
                );

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> AssignPermissionsToRoleAsync(
            Guid roleId,
            IEnumerable<Guid> permissionIds,
            Guid? operatorId = null
        )
        {
            var role = await _unitOfWork.Role.GetAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"Role ID '{roleId}' not found");
            }

            var permissions = await _unitOfWork.Permission.GetByIdsAsync(permissionIds);
            var existingIds = permissions.Select(p => p.Id).ToList();
            var missingIds = permissionIds.Except(existingIds).ToList();

            if (missingIds.Any())
            {
                throw new InvalidOperationException(
                    $"Permission ID {string.Join(", ", missingIds)} not found"
                );
            }

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.RolePermission.AddRolePermissionsAsync(
                    roleId,
                    permissionIds,
                    operatorId
                );

                // Record audit log
                await _unitOfWork.AuditLog.InsertAsync(
                    new Domain.Aggregates.Administration.AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "AssignPermissions",
                        EntityType = "Role",
                        EntityId = roleId,
                        Description = $"Assigned {permissionIds.Count()} permission(s) to role '{role.Name}'",
                        IpAddress = null,
                        UserAgent = null,
                        StatusCode = 200,
                        UserId = operatorId,
                        Username = null,
                        CreatedAt = DateTime.UtcNow,
                    }
                );

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> RemovePermissionsFromRoleAsync(
            Guid roleId,
            IEnumerable<Guid> permissionIds
        )
        {
            var role = await _unitOfWork.Role.GetAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"Role ID '{roleId}' not found");
            }

            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.RolePermission.RemoveRolePermissionsAsync(roleId, permissionIds);

                // Record audit log
                await _unitOfWork.AuditLog.InsertAsync(
                    new Domain.Aggregates.Administration.AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "RemovePermissions",
                        EntityType = "Role",
                        EntityId = roleId,
                        Description = $"Removed {permissionIds.Count()} permission(s) from role '{role.Name}'",
                        IpAddress = null,
                        UserAgent = null,
                        StatusCode = 200,
                        UserId = null,
                        Username = null,
                        CreatedAt = DateTime.UtcNow,
                    }
                );

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
            // Get all roles for the user
            var userRoles = await _unitOfWork.UserRole.GetUserRolesAsync(userId);

            if (!userRoles.Any())
            {
                return new List<string>();
            }

            // Get permissions for all roles (deduplicated)
            var allPermissions = new List<Permission>();
            foreach (var role in userRoles)
            {
                var rolePermissions = await _unitOfWork.Permission.GetByRoleIdAsync(role.Id);
                allPermissions.AddRange(rolePermissions);
            }

            // Deduplicate and return permission codes
            return allPermissions.Where(p => p.IsActive).Select(p => p.Code).Distinct().ToList();
        }
    }

    // Extension methods
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
                UpdatedAt = permission.UpdatedAt,
            };
        }
    }
}
