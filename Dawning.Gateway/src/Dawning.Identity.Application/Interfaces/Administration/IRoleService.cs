using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// 角色服务接口
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        Task<RoleDto?> GetAsync(Guid id);

        /// <summary>
        /// 根据名称获取角色
        /// </summary>
        Task<RoleDto?> GetByNameAsync(string name);

        /// <summary>
        /// 获取分页角色列表
        /// </summary>
        Task<PagedData<RoleDto>> GetPagedListAsync(RoleModel model, int page, int itemsPerPage);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        Task<IEnumerable<RoleDto>> GetAllAsync();

        /// <summary>
        /// 创建角色
        /// </summary>
        Task<RoleDto> CreateAsync(CreateRoleDto dto, Guid? operatorId = null);

        /// <summary>
        /// 更新角色
        /// </summary>
        Task<RoleDto> UpdateAsync(UpdateRoleDto dto, Guid? operatorId = null);

        /// <summary>
        /// 删除角色
        /// </summary>
        Task<bool> DeleteAsync(Guid id, Guid? operatorId = null);

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null);
    }
}
