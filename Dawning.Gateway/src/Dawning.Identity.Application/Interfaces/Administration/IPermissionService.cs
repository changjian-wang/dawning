using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// 权限服务接口
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
        /// 获取所有资源类型
        /// </summary>
        Task<IEnumerable<string>> GetResourceTypesAsync();

        /// <summary>
        /// 获取所有分类
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
