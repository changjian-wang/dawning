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
    /// IP 访问规则仓储实现
    /// </summary>
    public class IpAccessRuleRepository : IIpAccessRuleRepository
    {
        private readonly DbContext _context;

        public IpAccessRuleRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID获取规则
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
        /// 获取分页规则列表
        /// </summary>
        public async Task<PagedData<IpAccessRule>> GetPagedListAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        )
        {
            var builder = _context.Connection.Builder<IpAccessRuleEntity>(_context.Transaction);

            // 应用过滤条件
            builder = builder
                .WhereIf(!string.IsNullOrEmpty(ruleType), r => r.RuleType == ruleType)
                .WhereIf(isEnabled.HasValue, r => r.IsEnabled == isEnabled!.Value);

            // 按创建时间降序排序
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
        /// 获取指定类型的活跃规则
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
        /// 检查IP是否在黑名单中
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
        /// 检查IP是否在白名单中
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
        /// 插入规则
        /// </summary>
        public async ValueTask<int> InsertAsync(IpAccessRule model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 更新规则
        /// </summary>
        public async ValueTask<bool> UpdateAsync(IpAccessRule model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 删除规则
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
        /// 匹配IP地址（支持通配符）
        /// </summary>
        private static bool MatchIpAddress(string ipAddress, string pattern)
        {
            if (ipAddress == pattern)
                return true;

            // 支持通配符匹配，如 192.168.1.*
            if (pattern.Contains('*'))
            {
                var regex = "^" + pattern.Replace(".", "\\.").Replace("*", ".*") + "$";
                return System.Text.RegularExpressions.Regex.IsMatch(ipAddress, regex);
            }

            return false;
        }
    }
}
