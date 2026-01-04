using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// Role service interface
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Get role by ID
        /// </summary>
        Task<RoleDto?> GetAsync(Guid id);

        /// <summary>
        /// Get role by name
        /// </summary>
        Task<RoleDto?> GetByNameAsync(string name);

        /// <summary>
        /// Get paged role list
        /// </summary>
        Task<PagedData<RoleDto>> GetPagedListAsync(RoleModel model, int page, int itemsPerPage);

        /// <summary>
        /// Get all roles
        /// </summary>
        Task<IEnumerable<RoleDto>> GetAllAsync();

        /// <summary>
        /// Create role
        /// </summary>
        Task<RoleDto> CreateAsync(CreateRoleDto dto, Guid? operatorId = null);

        /// <summary>
        /// Update role
        /// </summary>
        Task<RoleDto> UpdateAsync(UpdateRoleDto dto, Guid? operatorId = null);

        /// <summary>
        /// Delete role
        /// </summary>
        Task<bool> DeleteAsync(Guid id, Guid? operatorId = null);

        /// <summary>
        /// Check if role name exists
        /// </summary>
        Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null);
    }
}
