using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.MultiTenancy;
using Dawning.Identity.Infra.Data.PersistentObjects.MultiTenancy;
using Dawning.ORM.Dapper;
using static Dawning.ORM.Dapper.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.MultiTenancy
{
    /// <summary>
    /// Tenant repository implementation
    /// </summary>
    public class TenantRepository : ITenantRepository
    {
        private readonly DbContext _context;

        public TenantRepository(DbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<TenantEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetByCodeAsync(string code)
        {
            var entity = await _context
                .Connection.Builder<TenantEntity>(_context.Transaction)
                .Where(t => t.Code == code)
                .FirstOrDefaultAsync();
            return entity?.ToModel();
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetByDomainAsync(string domain)
        {
            var entity = await _context
                .Connection.Builder<TenantEntity>(_context.Transaction)
                .Where(t => t.Domain == domain)
                .FirstOrDefaultAsync();
            return entity?.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            var entities = await _context
                .Connection.Builder<TenantEntity>(_context.Transaction)
                .OrderBy(t => t.Name)
                .ToListAsync();
            return entities.Select(e => e.ToModel());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
        {
            var entities = await _context
                .Connection.Builder<TenantEntity>(_context.Transaction)
                .Where(t => t.IsActive == true)
                .OrderBy(t => t.Name)
                .ToListAsync();
            return entities.Select(e => e.ToModel());
        }

        /// <inheritdoc/>
        public async Task<PagedData<Tenant>> GetPagedAsync(
            string? keyword,
            bool? isActive,
            int page,
            int pageSize
        )
        {
            var builder = _context.Connection.Builder<TenantEntity>(_context.Transaction);

            // Keyword search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                builder.Where(t =>
                    t.Name.Contains(keyword)
                    || t.Code.Contains(keyword)
                    || (t.Email != null && t.Email.Contains(keyword))
                );
            }

            // Enabled status filter
            if (isActive.HasValue)
            {
                builder.Where(t => t.IsActive == isActive.Value);
            }

            var result = await builder
                .OrderByDescending(t => t.CreatedAt)
                .AsPagedListAsync(page, pageSize);

            return new PagedData<Tenant>
            {
                Items = result.Values.Select(e => e.ToModel()).ToList(),
                TotalCount = result.TotalItems,
                PageIndex = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc/>
        public async Task<int> InsertAsync(Tenant tenant)
        {
            var entity = tenant.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <inheritdoc/>
        public async Task<int> UpdateAsync(Tenant tenant)
        {
            var entity = tenant.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction) ? 1 : 0;
        }

        /// <inheritdoc/>
        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<TenantEntity>(id, _context.Transaction);
            if (entity == null)
                return 0;
            return await _context.Connection.DeleteAsync(entity, _context.Transaction) ? 1 : 0;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null)
        {
            var builder = _context
                .Connection.Builder<TenantEntity>(_context.Transaction)
                .Where(t => t.Code == code);

            if (excludeId.HasValue)
            {
                builder.Where(t => t.Id != excludeId.Value);
            }

            var entity = await builder.FirstOrDefaultAsync();
            return entity != null;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsDomainAsync(string domain, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            var builder = _context
                .Connection.Builder<TenantEntity>(_context.Transaction)
                .Where(t => t.Domain == domain);

            if (excludeId.HasValue)
            {
                builder.Where(t => t.Id != excludeId.Value);
            }

            var entity = await builder.FirstOrDefaultAsync();
            return entity != null;
        }
    }
}
