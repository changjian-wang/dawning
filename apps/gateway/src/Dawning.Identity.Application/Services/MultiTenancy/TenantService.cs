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
    /// Tenant service implementation
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
            // Validate code uniqueness
            if (await _uow.Tenant.ExistsCodeAsync(tenant.Code))
            {
                throw new InvalidOperationException($"Tenant code '{tenant.Code}' already exists");
            }

            // Validate domain uniqueness
            if (
                !string.IsNullOrWhiteSpace(tenant.Domain)
                && await _uow.Tenant.ExistsDomainAsync(tenant.Domain)
            )
            {
                throw new InvalidOperationException($"Domain '{tenant.Domain}' is already in use");
            }

            tenant.Id = Guid.NewGuid();
            tenant.CreatedAt = DateTime.UtcNow;

            await _uow.Tenant.InsertAsync(tenant);
            _logger.LogInformation("Created tenant: {TenantCode} ({TenantId})", tenant.Code, tenant.Id);

            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant> UpdateAsync(Tenant tenant)
        {
            var existing = await _uow.Tenant.GetAsync(tenant.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Tenant not found: {tenant.Id}");
            }

            // Validate code uniqueness
            if (await _uow.Tenant.ExistsCodeAsync(tenant.Code, tenant.Id))
            {
                throw new InvalidOperationException($"Tenant code '{tenant.Code}' already exists");
            }

            // Validate domain uniqueness
            if (
                !string.IsNullOrWhiteSpace(tenant.Domain)
                && await _uow.Tenant.ExistsDomainAsync(tenant.Domain, tenant.Id)
            )
            {
                throw new InvalidOperationException($"Domain '{tenant.Domain}' is already in use");
            }

            tenant.UpdatedAt = DateTime.UtcNow;
            await _uow.Tenant.UpdateAsync(tenant);

            // Clear cache
            await InvalidateCacheAsync(existing);

            _logger.LogInformation("Updated tenant: {TenantCode} ({TenantId})", tenant.Code, tenant.Id);
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
                    "Deleted tenant: {TenantCode} ({TenantId})",
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
                "Set tenant status: {TenantCode} ({TenantId}) -> {IsActive}",
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
            // 1. Prioritize resolution by tenant code
            if (!string.IsNullOrWhiteSpace(tenantCode))
            {
                var tenant = await GetByCodeAsync(tenantCode);
                if (tenant != null && tenant.IsActive)
                {
                    return tenant;
                }
            }

            // 2. Resolve by domain
            if (!string.IsNullOrWhiteSpace(host))
            {
                // Remove port number
                var domain = host.Split(':')[0];

                // Try full domain matching
                var tenant = await GetByDomainAsync(domain);
                if (tenant != null && tenant.IsActive)
                {
                    return tenant;
                }

                // Try subdomain matching (e.g., tenant1.example.com -> tenant1)
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
