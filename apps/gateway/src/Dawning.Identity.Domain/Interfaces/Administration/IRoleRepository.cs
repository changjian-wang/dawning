using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// Role repository interface
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Asynchronously get role by ID
        /// </summary>
        Task<Role?> GetAsync(Guid id);

        /// <summary>
        /// Get role by name
        /// </summary>
        Task<Role?> GetByNameAsync(string name);

        /// <summary>
        /// Get paginated role list
        /// </summary>
        Task<PagedData<Role>> GetPagedListAsync(RoleModel model, int page, int itemsPerPage);

        /// <summary>
        /// Get all roles
        /// </summary>
        Task<IEnumerable<Role>> GetAllAsync();

        /// <summary>
        /// Asynchronously insert role
        /// </summary>
        ValueTask<int> InsertAsync(Role model);

        /// <summary>
        /// Asynchronously update role
        /// </summary>
        ValueTask<bool> UpdateAsync(Role model);

        /// <summary>
        /// Asynchronously delete role (soft delete)
        /// </summary>
        ValueTask<bool> DeleteAsync(Role model);

        /// <summary>
        /// Check if role name exists
        /// </summary>
        Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null);
    }
}
