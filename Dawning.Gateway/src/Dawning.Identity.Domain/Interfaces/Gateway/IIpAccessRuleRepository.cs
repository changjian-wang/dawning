using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// IP 访问规则仓储接口
    /// </summary>
    public interface IIpAccessRuleRepository
    {
        /// <summary>
        /// 根据ID获取规则
        /// </summary>
        Task<IpAccessRule?> GetAsync(Guid id);

        /// <summary>
        /// 获取分页规则列表
        /// </summary>
        Task<PagedData<IpAccessRule>> GetPagedListAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        );

        /// <summary>
        /// 获取指定类型的活跃规则
        /// </summary>
        Task<IEnumerable<IpAccessRule>> GetActiveRulesByTypeAsync(string ruleType);

        /// <summary>
        /// 检查IP是否在黑名单中
        /// </summary>
        Task<bool> IsIpBlacklistedAsync(string ipAddress);

        /// <summary>
        /// 检查IP是否在白名单中
        /// </summary>
        Task<bool> IsIpWhitelistedAsync(string ipAddress);

        /// <summary>
        /// 插入规则
        /// </summary>
        ValueTask<int> InsertAsync(IpAccessRule model);

        /// <summary>
        /// 更新规则
        /// </summary>
        ValueTask<bool> UpdateAsync(IpAccessRule model);

        /// <summary>
        /// 删除规则
        /// </summary>
        ValueTask<bool> DeleteAsync(Guid id);
    }
}
