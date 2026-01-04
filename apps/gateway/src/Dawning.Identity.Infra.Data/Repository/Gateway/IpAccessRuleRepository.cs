using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Gateway;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Gateway
{
    /// <summary>
    /// IP access rule repository implementation
    /// </summary>
    public class IpAccessRuleRepository : IIpAccessRuleRepository
    {
        private readonly DbContext _context;

        public IpAccessRuleRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get rule by ID
        /// </summary>
        public async Task<IpAccessRule?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<IpAccessRuleEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel();
        }

        /// <summary>
        /// Get paged rule list
        /// </summary>
        public async Task<PagedData<IpAccessRule>> GetPagedListAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        )
        {
            var builder = _context.Connection.Builder<IpAccessRuleEntity>(_context.Transaction);

            // Apply filter conditions
            builder = builder
                .WhereIf(!string.IsNullOrEmpty(ruleType), r => r.RuleType == ruleType)
                .WhereIf(isEnabled.HasValue, r => r.IsEnabled == isEnabled!.Value);

            // Order by created time descending
            var result = await builder
                .OrderByDescending(r => r.CreatedAt)
                .AsPagedListAsync(page, pageSize);

            return new PagedData<IpAccessRule>
            {
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };
        }

        /// <summary>
        /// Get active rules by type
        /// </summary>
        public async Task<IEnumerable<IpAccessRule>> GetActiveRulesByTypeAsync(string ruleType)
        {
            var now = DateTime.UtcNow;
            var entities = await _context
                .Connection.Builder<IpAccessRuleEntity>(_context.Transaction)
                .Where(r => r.RuleType == ruleType)
                .Where(r => r.IsEnabled == true)
                .Where(r => r.ExpiresAt == null || r.ExpiresAt > now)
                .AsListAsync();

            return entities.ToModels();
        }

        /// <summary>
        /// Check if IP is in blacklist
        /// </summary>
        public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            var now = DateTime.UtcNow;
            var rules = await _context
                .Connection.Builder<IpAccessRuleEntity>(_context.Transaction)
                .Where(r => r.RuleType == "blacklist")
                .Where(r => r.IsEnabled == true)
                .Where(r => r.ExpiresAt == null || r.ExpiresAt > now)
                .AsListAsync();

            return rules.Any(r => MatchIpAddress(ipAddress, r.IpAddress));
        }

        /// <summary>
        /// Check if IP is in whitelist
        /// </summary>
        public async Task<bool> IsIpWhitelistedAsync(string ipAddress)
        {
            var now = DateTime.UtcNow;
            var rules = await _context
                .Connection.Builder<IpAccessRuleEntity>(_context.Transaction)
                .Where(r => r.RuleType == "whitelist")
                .Where(r => r.IsEnabled == true)
                .Where(r => r.ExpiresAt == null || r.ExpiresAt > now)
                .AsListAsync();

            return rules.Any(r => MatchIpAddress(ipAddress, r.IpAddress));
        }

        /// <summary>
        /// Insert rule
        /// </summary>
        public async ValueTask<int> InsertAsync(IpAccessRule model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// Update rule
        /// </summary>
        public async ValueTask<bool> UpdateAsync(IpAccessRule model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// Delete rule
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<IpAccessRuleEntity>(
                id,
                _context.Transaction
            );
            if (entity == null)
                return false;

            var result = await _context.Connection.DeleteAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// Match IP address (supports wildcards)
        /// </summary>
        private static bool MatchIpAddress(string ipAddress, string pattern)
        {
            if (ipAddress == pattern)
                return true;

            // Support wildcard matching, e.g., 192.168.1.*
            if (pattern.Contains('*'))
            {
                var regex = "^" + pattern.Replace(".", "\\.").Replace("*", ".*") + "$";
                return System.Text.RegularExpressions.Regex.IsMatch(ipAddress, regex);
            }

            return false;
        }
    }
}
