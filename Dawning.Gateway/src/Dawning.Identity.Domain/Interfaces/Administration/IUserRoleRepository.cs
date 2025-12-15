using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// 用户角色关联仓储接口
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// 获取用户的所有角色
        /// </summary>
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// 获取拥有指定角色的所有用户
        /// </summary>
        Task<IEnumerable<User>> GetRoleUsersAsync(Guid roleId);

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid? operatorId = null);

        /// <summary>
        /// 批量为用户分配角色
        /// </summary>
        Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        );

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// 移除用户的所有角色
        /// </summary>
        Task<bool> RemoveAllRolesAsync(Guid userId);

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        Task<bool> HasRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// 检查用户是否拥有指定角色（按角色名称）
        /// </summary>
        Task<bool> HasRoleByNameAsync(Guid userId, string roleName);
    }
}
