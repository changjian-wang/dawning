using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// Permission service interface
    /// </summary>
    public interface IPermissionService
    {
        Task<PermissionDto?> GetAsync(Guid id);
        Task<PermissionDto?> GetByCodeAsync(string code);
        Task<PagedData<PermissionDto>> GetPagedListAsync(
            PermissionModel model,
            int page,
            int pageSize
        );
        Task<IEnumerable<PermissionDto>> GetAllAsync();
        Task<IEnumerable<PermissionDto>> GetByRoleIdAsync(Guid roleId);
        Task<IEnumerable<PermissionGroupDto>> GetGroupedPermissionsAsync();

        /// <summary>
        /// Get all resource types
        /// </summary>
        Task<IEnumerable<string>> GetResourceTypesAsync();

        /// <summary>
        /// Get all categories
        /// </summary>
        Task<IEnumerable<string>> GetCategoriesAsync();

        Task<PermissionDto> CreateAsync(CreatePermissionDto dto, Guid? operatorId = null);
        Task<PermissionDto> UpdateAsync(UpdatePermissionDto dto, Guid? operatorId = null);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> AssignPermissionsToRoleAsync(
            Guid roleId,
            IEnumerable<Guid> permissionIds,
            Guid? operatorId = null
        );
        Task<bool> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds);
        Task<bool> HasPermissionAsync(Guid roleId, string permissionCode);
        Task<IEnumerable<string>> GetUserPermissionCodesAsync(Guid userId);
    }
}
