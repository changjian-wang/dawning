using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// 角色仓储接口
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// 根据ID异步获取角色
        /// </summary>
        Task<Role?> GetAsync(Guid id);

        /// <summary>
        /// 根据角色名称获取角色
        /// </summary>
        Task<Role?> GetByNameAsync(string name);

        /// <summary>
        /// 获取分页角色列表
        /// </summary>
        Task<PagedData<Role>> GetPagedListAsync(RoleModel model, int page, int itemsPerPage);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        Task<IEnumerable<Role>> GetAllAsync();

        /// <summary>
        /// 异步插入角色
        /// </summary>
        ValueTask<int> InsertAsync(Role model);

        /// <summary>
        /// 异步更新角色
        /// </summary>
        ValueTask<bool> UpdateAsync(Role model);

        /// <summary>
        /// 异步删除角色（软删除）
        /// </summary>
        ValueTask<bool> DeleteAsync(Role model);

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null);
    }
}
