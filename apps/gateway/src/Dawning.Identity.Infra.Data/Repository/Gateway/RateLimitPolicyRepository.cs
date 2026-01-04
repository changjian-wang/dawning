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
    /// Rate limit policy repository implementation
    /// </summary>
    public class RateLimitPolicyRepository : IRateLimitPolicyRepository
    {
        private readonly DbContext _context;

        public RateLimitPolicyRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get policy by ID
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
        /// Get policy by name
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
        /// Get all policies
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
        /// Get all enabled policies
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
        /// Insert policy
        /// </summary>
        public async ValueTask<int> InsertAsync(RateLimitPolicy model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// Update policy
        /// </summary>
        public async ValueTask<bool> UpdateAsync(RateLimitPolicy model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// Delete policy
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
