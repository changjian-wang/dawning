using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Models;

namespace Dawning.Identity.Domain.Interfaces.Gateway
{
    /// <summary>
    /// 限流策略仓储接口
    /// </summary>
    public interface IRateLimitPolicyRepository
    {
        /// <summary>
        /// 根据ID获取策略
        /// </summary>
        Task<RateLimitPolicy?> GetAsync(Guid id);

        /// <summary>
        /// 根据名称获取策略
        /// </summary>
        Task<RateLimitPolicy?> GetByNameAsync(string name);

        /// <summary>
        /// 获取所有策略
        /// </summary>
        Task<IEnumerable<RateLimitPolicy>> GetAllAsync();

        /// <summary>
        /// 获取所有启用的策略
        /// </summary>
        Task<IEnumerable<RateLimitPolicy>> GetAllEnabledAsync();

        /// <summary>
        /// 插入策略
        /// </summary>
        ValueTask<int> InsertAsync(RateLimitPolicy model);

        /// <summary>
        /// 更新策略
        /// </summary>
        ValueTask<bool> UpdateAsync(RateLimitPolicy model);

        /// <summary>
        /// 删除策略
        /// </summary>
        ValueTask<bool> DeleteAsync(Guid id);
    }
}
