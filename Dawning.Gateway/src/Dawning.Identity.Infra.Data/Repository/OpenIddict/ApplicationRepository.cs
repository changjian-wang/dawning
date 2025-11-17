using System;
using Dapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.Repository.OpenIddict
{
    /// <summary>
    /// Application Repository 实现（参考 ClaimTypeRepository 模式）
    /// </summary>
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly DbContext _context;

        public ApplicationRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Application?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<ApplicationEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        public async Task<Application?> GetByClientIdAsync(string clientId)
        {
            var sql = "SELECT * FROM openiddict_applications WHERE client_id = @ClientId";
            var entity = await _context.Connection.QueryFirstOrDefaultAsync<ApplicationEntity>(
                sql,
                new { ClientId = clientId },
                _context.Transaction
            );
            return entity?.ToModel();
        }

        public async Task<IEnumerable<Application>> GetAllAsync()
        {
            var entities = await _context.Connection.GetAllAsync<ApplicationEntity>(_context.Transaction);
            return entities?.ToModels() ?? Enumerable.Empty<Application>();
        }

        public async Task<int> InsertAsync(Application application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var entity = application.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        public async Task<bool> UpdateAsync(Application application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var entity = application.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _context.Connection.DeleteAsync(new ApplicationEntity { Id = id }, _context.Transaction);
        }

        public async Task<long> CountAsync()
        {
            var sql = "SELECT COUNT(*) FROM openiddict_applications";
            return await _context.Connection.ExecuteScalarAsync<long>(sql, transaction: _context.Transaction);
        }
    }
}

