using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.MultiTenancy
{
    /// <summary>
    /// 租户服务实现
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork _uow;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TenantService> _logger;
        private const string CachePrefix = "tenant:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

        public TenantService(
            IUnitOfWork uow,
            IDistributedCache cache,
            ILogger<TenantService> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetAsync(Guid id)
        {
            var cacheKey = $"{CachePrefix}id:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<Tenant>(cached);
            }

            var tenant = await _uow.Tenant.GetAsync(id);
            if (tenant != null)
            {
                await CacheTenantAsync(tenant);
            }
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetByCodeAsync(string code)
        {
            var cacheKey = $"{CachePrefix}code:{code}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<Tenant>(cached);
            }

            var tenant = await _uow.Tenant.GetByCodeAsync(code);
            if (tenant != null)
            {
                await CacheTenantAsync(tenant);
            }
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetByDomainAsync(string domain)
        {
            var cacheKey = $"{CachePrefix}domain:{domain}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<Tenant>(cached);
            }

            var tenant = await _uow.Tenant.GetByDomainAsync(domain);
            if (tenant != null)
            {
                await CacheTenantAsync(tenant);
            }
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            return await _uow.Tenant.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
        {
            return await _uow.Tenant.GetActiveTenantsAsync();
        }

        /// <inheritdoc/>
        public async Task<PagedData<Tenant>> GetPagedAsync(
            string? keyword,
            bool? isActive,
            int page,
            int pageSize
        )
        {
            return await _uow.Tenant.GetPagedAsync(keyword, isActive, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<Tenant> CreateAsync(Tenant tenant)
        {
            // 验证代码唯一性
            if (await _uow.Tenant.ExistsCodeAsync(tenant.Code))
            {
                throw new InvalidOperationException($"租户代码 '{tenant.Code}' 已存在");
            }

            // 验证域名唯一性
            if (
                !string.IsNullOrWhiteSpace(tenant.Domain)
                && await _uow.Tenant.ExistsDomainAsync(tenant.Domain)
            )
            {
                throw new InvalidOperationException($"域名 '{tenant.Domain}' 已被使用");
            }

            tenant.Id = Guid.NewGuid();
            tenant.CreatedAt = DateTime.UtcNow;

            await _uow.Tenant.InsertAsync(tenant);
            _logger.LogInformation("创建租户: {TenantCode} ({TenantId})", tenant.Code, tenant.Id);

            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant> UpdateAsync(Tenant tenant)
        {
            var existing = await _uow.Tenant.GetAsync(tenant.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"租户不存在: {tenant.Id}");
            }

            // 验证代码唯一性
            if (await _uow.Tenant.ExistsCodeAsync(tenant.Code, tenant.Id))
            {
                throw new InvalidOperationException($"租户代码 '{tenant.Code}' 已存在");
            }

            // 验证域名唯一性
            if (
                !string.IsNullOrWhiteSpace(tenant.Domain)
                && await _uow.Tenant.ExistsDomainAsync(tenant.Domain, tenant.Id)
            )
            {
                throw new InvalidOperationException($"域名 '{tenant.Domain}' 已被使用");
            }

            tenant.UpdatedAt = DateTime.UtcNow;
            await _uow.Tenant.UpdateAsync(tenant);

            // 清除缓存
            await InvalidateCacheAsync(existing);

            _logger.LogInformation("更新租户: {TenantCode} ({TenantId})", tenant.Code, tenant.Id);
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var tenant = await _uow.Tenant.GetAsync(id);
            if (tenant == null)
            {
                return false;
            }

            var result = await _uow.Tenant.DeleteAsync(id);
            if (result > 0)
            {
                await InvalidateCacheAsync(tenant);
                _logger.LogInformation(
                    "删除租户: {TenantCode} ({TenantId})",
                    tenant.Code,
                    tenant.Id
                );
            }
            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> SetActiveAsync(Guid id, bool isActive)
        {
            var tenant = await _uow.Tenant.GetAsync(id);
            if (tenant == null)
            {
                return false;
            }

            tenant.IsActive = isActive;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _uow.Tenant.UpdateAsync(tenant);
            await InvalidateCacheAsync(tenant);

            _logger.LogInformation(
                "设置租户状态: {TenantCode} ({TenantId}) -> {IsActive}",
                tenant.Code,
                tenant.Id,
                isActive
            );
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null)
        {
            return !await _uow.Tenant.ExistsCodeAsync(code, excludeId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsDomainAvailableAsync(string domain, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return true;
            return !await _uow.Tenant.ExistsDomainAsync(domain, excludeId);
        }

        /// <inheritdoc/>
        public async Task<Tenant?> ResolveTenantAsync(string? tenantCode, string? host)
        {
            // 1. 优先通过租户代码解析
            if (!string.IsNullOrWhiteSpace(tenantCode))
            {
                var tenant = await GetByCodeAsync(tenantCode);
                if (tenant != null && tenant.IsActive)
                {
                    return tenant;
                }
            }

            // 2. 通过域名解析
            if (!string.IsNullOrWhiteSpace(host))
            {
                // 移除端口号
                var domain = host.Split(':')[0];

                // 尝试完整域名匹配
                var tenant = await GetByDomainAsync(domain);
                if (tenant != null && tenant.IsActive)
                {
                    return tenant;
                }

                // 尝试子域名匹配 (如 tenant1.example.com -> tenant1)
                var parts = domain.Split('.');
                if (parts.Length >= 2)
                {
                    var subdomain = parts[0];
                    tenant = await GetByCodeAsync(subdomain);
                    if (tenant != null && tenant.IsActive)
                    {
                        return tenant;
                    }
                }
            }

            return null;
        }

        private async Task CacheTenantAsync(Tenant tenant)
        {
            var json = JsonSerializer.Serialize(tenant);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
            };

            await _cache.SetStringAsync($"{CachePrefix}id:{tenant.Id}", json, options);
            await _cache.SetStringAsync($"{CachePrefix}code:{tenant.Code}", json, options);
            if (!string.IsNullOrWhiteSpace(tenant.Domain))
            {
                await _cache.SetStringAsync($"{CachePrefix}domain:{tenant.Domain}", json, options);
            }
        }

        private async Task InvalidateCacheAsync(Tenant tenant)
        {
            await _cache.RemoveAsync($"{CachePrefix}id:{tenant.Id}");
            await _cache.RemoveAsync($"{CachePrefix}code:{tenant.Code}");
            if (!string.IsNullOrWhiteSpace(tenant.Domain))
            {
                await _cache.RemoveAsync($"{CachePrefix}domain:{tenant.Domain}");
            }
        }
    }
}
