using Dapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;
using Dawning.Shared.Dapper.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Infra.Data.Repository.OpenIddict
{
    /// <summary>
    /// Scope Repository 实现
    /// </summary>
    public class ScopeRepository : IScopeRepository
    {
        private readonly DbContext _context;

        public ScopeRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Scope?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<ScopeEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        public async Task<Scope?> GetByNameAsync(string name)
        {
            var sql = "SELECT * FROM openiddict_scopes WHERE name = @Name";
            var entity = await _context.Connection.QueryFirstOrDefaultAsync<ScopeEntity>(
                sql,
                new { Name = name },
                _context.Transaction
            );
            return entity?.ToModel();
        }

        public async Task<IEnumerable<Scope>> GetAllAsync()
        {
            var entities = await _context.Connection.GetAllAsync<ScopeEntity>(_context.Transaction);
            return entities?.ToModels() ?? Enumerable.Empty<Scope>();
        }

        public async Task<IEnumerable<Scope>> GetByNamesAsync(IEnumerable<string> names)
        {
            if (names == null || !names.Any())
                return Enumerable.Empty<Scope>();

            var sql = "SELECT * FROM openiddict_scopes WHERE name = ANY(@Names)";
            var entities = await _context.Connection.QueryAsync<ScopeEntity>(
                sql,
                new { Names = names.ToArray() },
                _context.Transaction
            );
            return entities?.ToModels() ?? Enumerable.Empty<Scope>();
        }

        public async Task<int> InsertAsync(Scope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            var entity = scope.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        public async Task<bool> UpdateAsync(Scope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            var entity = scope.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _context.Connection.DeleteAsync(new ScopeEntity { Id = id }, _context.Transaction);
        }

        public async Task<long> CountAsync()
        {
            var sql = "SELECT COUNT(*) FROM openiddict_scopes";
            return await _context.Connection.ExecuteScalarAsync<long>(sql, transaction: _context.Transaction);
        }
    }
}
