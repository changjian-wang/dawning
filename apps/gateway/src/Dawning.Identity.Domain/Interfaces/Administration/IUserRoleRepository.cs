using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// User role association repository interface
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Get all roles for a user
        /// </summary>
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Get all users with the specified role
        /// </summary>
        Task<IEnumerable<User>> GetRoleUsersAsync(Guid roleId);

        /// <summary>
        /// Assign role to user
        /// </summary>
        Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid? operatorId = null);

        /// <summary>
        /// Batch assign roles to user
        /// </summary>
        Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        );

        /// <summary>
        /// Remove role from user
        /// </summary>
        Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// Remove all roles from user
        /// </summary>
        Task<bool> RemoveAllRolesAsync(Guid userId);

        /// <summary>
        /// Check if user has the specified role
        /// </summary>
        Task<bool> HasRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// Check if user has the specified role (by role name)
        /// </summary>
        Task<bool> HasRoleByNameAsync(Guid userId, string roleName);
    }
}
