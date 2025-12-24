using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Gateway
{
    /// <summary>
    /// 限流策略仓储实现
    /// </summary>
    public class RateLimitPolicyRepository : IRateLimitPolicyRepository
    {
        private readonly DbContext _context;

        public RateLimitPolicyRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID获取策略
        /// </summary>
        public async Task<RateLimitPolicy?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<RateLimitPolicyEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// 根据名称获取策略
        /// </summary>
        public async Task<RateLimitPolicy?> GetByNameAsync(string name)
        {
            var result = await _context
                .Connection.Builder<RateLimitPolicyEntity>(_context.Transaction)
                .Where(r => r.Name == name)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// 获取所有策略
        /// </summary>
        public async Task<IEnumerable<RateLimitPolicy>> GetAllAsync()
        {
            var entities = await _context
                .Connection.Builder<RateLimitPolicyEntity>(_context.Transaction)
                .OrderByDescending(r => r.CreatedAt)
                .AsListAsync();

            return entities.ToModels();
        }

        /// <summary>
        /// 获取所有启用的策略
        /// </summary>
        public async Task<IEnumerable<RateLimitPolicy>> GetAllEnabledAsync()
        {
            var entities = await _context
                .Connection.Builder<RateLimitPolicyEntity>(_context.Transaction)
                .Where(r => r.IsEnabled == true)
                .OrderBy(r => r.Name)
                .AsListAsync();

            return entities.ToModels();
        }

        /// <summary>
        /// 插入策略
        /// </summary>
        public async ValueTask<int> InsertAsync(RateLimitPolicy model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 更新策略
        /// </summary>
        public async ValueTask<bool> UpdateAsync(RateLimitPolicy model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 删除策略
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<RateLimitPolicyEntity>(
                id,
                _context.Transaction
            );
            if (entity == null)
                return false;

            var result = await _context.Connection.DeleteAsync(entity, _context.Transaction);
            return result;
        }
    }
}
