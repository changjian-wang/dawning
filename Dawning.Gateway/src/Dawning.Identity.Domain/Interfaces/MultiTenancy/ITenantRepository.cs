using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Domain.Interfaces.MultiTenancy
{
    /// <summary>
    /// 租户仓储接口
    /// </summary>
    public interface ITenantRepository
    {
        /// <summary>
        /// 根据ID获取租户
        /// </summary>
        Task<Tenant?> GetAsync(Guid id);

        /// <summary>
        /// 根据租户代码获取租户
        /// </summary>
        Task<Tenant?> GetByCodeAsync(string code);

        /// <summary>
        /// 根据域名获取租户
        /// </summary>
        Task<Tenant?> GetByDomainAsync(string domain);

        /// <summary>
        /// 获取所有租户列表
        /// </summary>
        Task<IEnumerable<Tenant>> GetAllAsync();

        /// <summary>
        /// 获取所有启用的租户
        /// </summary>
        Task<IEnumerable<Tenant>> GetActiveTenantsAsync();

        /// <summary>
        /// 分页获取租户列表
        /// </summary>
        Task<PagedData<Tenant>> GetPagedAsync(
            string? keyword,
            bool? isActive,
            int page,
            int pageSize
        );

        /// <summary>
        /// 创建租户
        /// </summary>
        Task<int> InsertAsync(Tenant tenant);

        /// <summary>
        /// 更新租户
        /// </summary>
        Task<int> UpdateAsync(Tenant tenant);

        /// <summary>
        /// 删除租户
        /// </summary>
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// 检查租户代码是否存在
        /// </summary>
        Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null);

        /// <summary>
        /// 检查域名是否存在
        /// </summary>
        Task<bool> ExistsDomainAsync(string domain, Guid? excludeId = null);
    }
}
