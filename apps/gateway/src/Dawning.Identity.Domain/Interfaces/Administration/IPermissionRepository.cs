using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// Permission repository interface
    /// </summary>
    public interface IPermissionRepository
    {
        Task<Permission?> GetAsync(Guid id);
        Task<Permission?> GetByCodeAsync(string code);
        Task<PagedData<Permission>> GetPagedListAsync(
            PermissionModel model,
            int page,
            int pageSize
        );
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId);
        Task<IEnumerable<Permission>> GetByResourceAsync(string resource);
        Task<bool> InsertAsync(Permission permission);
        Task<bool> UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(Permission permission);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    }

    /// <summary>
    /// Role permission repository interface
    /// </summary>
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId);
        Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId);
        Task<bool> AddRolePermissionsAsync(
            Guid roleId,
            IEnumerable<Guid> permissionIds,
            Guid? operatorId = null
        );
        Task<bool> RemoveRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
        Task<bool> RemoveAllRolePermissionsAsync(Guid roleId);
        Task<bool> HasPermissionAsync(Guid roleId, string permissionCode);
    }
}
