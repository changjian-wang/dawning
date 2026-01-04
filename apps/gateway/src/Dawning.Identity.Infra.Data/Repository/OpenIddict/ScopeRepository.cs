using Dapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;
using Dawning.ORM.Dapper;
using static Dawning.ORM.Dapper.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.OpenIddict
{
    /// <summary>
    /// Scope Repository implementation
    /// </summary>
    public class ScopeRepository : IScopeRepository
    {
        private readonly DbContext _context;

        public ScopeRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get Scope by ID asynchronously
        /// </summary>
        public async Task<Scope> GetAsync(Guid id)
        {
            ScopeEntity entity = await _context.Connection.GetAsync<ScopeEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel() ?? new Scope();
        }

        /// <summary>
        /// Get Scope by name asynchronously
        /// </summary>
        public async Task<Scope?> GetByNameAsync(string name)
        {
            var result = await _context
                .Connection.Builder<ScopeEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(name), s => s.Name == name)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// Get paged list
        /// </summary>
        public async Task<PagedData<Scope>> GetPagedListAsync(
            ScopeModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedResult<ScopeEntity> result = await _context
                .Connection.Builder<ScopeEntity>(_context.Transaction)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    s => s.Name!.Contains(model.Name ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.DisplayName),
                    s => s.DisplayName!.Contains(model.DisplayName ?? "")
                )
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<Scope> pagedData = new PagedData<Scope>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };

            return pagedData;
        }

        /// <summary>
        /// Get all Scopes
        /// </summary>
        public async Task<IEnumerable<Scope>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<ScopeEntity>(_context.Transaction);
            return list?.ToModels() ?? new List<Scope>();
        }

        /// <summary>
        /// Get Scopes by name list
        /// </summary>
        public async Task<IEnumerable<Scope>> GetByNamesAsync(IEnumerable<string> names)
        {
            if (names == null || !names.Any())
                return Enumerable.Empty<Scope>();

            // Use QueryBuilder's supported IN operation: collection.Contains(field)
            var result = await _context
                .Connection.Builder<ScopeEntity>(_context.Transaction)
                .WhereIf(true, s => names.Contains(s.Name!))
                .AsListAsync();

            return result?.ToModels() ?? new List<Scope>();
        }

        /// <summary>
        /// Insert Scope asynchronously
        /// </summary>
        public async ValueTask<int> InsertAsync(Scope model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ScopeEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Update Scope asynchronously
        /// </summary>
        public async ValueTask<bool> UpdateAsync(Scope model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ScopeEntity entity = model.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Delete Scope asynchronously
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Scope model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ScopeEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }
    }
}
