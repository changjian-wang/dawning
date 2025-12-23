using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Application.Interfaces.MultiTenancy
{
    /// <summary>
    /// 租户服务接口
    /// </summary>
    public interface ITenantService
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
        /// 获取所有租户
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
            int pageSize);

        /// <summary>
        /// 创建租户
        /// </summary>
        Task<Tenant> CreateAsync(Tenant tenant);

        /// <summary>
        /// 更新租户
        /// </summary>
        Task<Tenant> UpdateAsync(Tenant tenant);

        /// <summary>
        /// 删除租户
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 切换租户启用状态
        /// </summary>
        Task<bool> SetActiveAsync(Guid id, bool isActive);

        /// <summary>
        /// 检查租户代码是否可用
        /// </summary>
        Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null);

        /// <summary>
        /// 检查域名是否可用
        /// </summary>
        Task<bool> IsDomainAvailableAsync(string domain, Guid? excludeId = null);

        /// <summary>
        /// 解析租户（从请求头、域名、查询参数等）
        /// </summary>
        Task<Tenant?> ResolveTenantAsync(string? tenantCode, string? host);
    }
}
