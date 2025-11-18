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
    /// Authorization Repository 实现
    /// </summary>
    public class AuthorizationRepository : IAuthorizationRepository
    {
        private readonly DbContext _context;

        public AuthorizationRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Authorization?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<AuthorizationEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        public async Task<IEnumerable<Authorization>> GetBySubjectAsync(string subject)
        {
            var sql = "SELECT * FROM openiddict_authorizations WHERE subject = @Subject";
            var entities = await _context.Connection.QueryAsync<AuthorizationEntity>(
                sql,
                new { Subject = subject },
                _context.Transaction
            );
            return entities?.ToModels() ?? Enumerable.Empty<Authorization>();
        }

        public async Task<IEnumerable<Authorization>> GetByApplicationIdAsync(Guid applicationId)
        {
            var sql = "SELECT * FROM openiddict_authorizations WHERE application_id = @ApplicationId";
            var entities = await _context.Connection.QueryAsync<AuthorizationEntity>(
                sql,
                new { ApplicationId = applicationId },
                _context.Transaction
            );
            return entities?.ToModels() ?? Enumerable.Empty<Authorization>();
        }

        public async Task<IEnumerable<Authorization>> GetAllAsync()
        {
            var entities = await _context.Connection.GetAllAsync<AuthorizationEntity>(_context.Transaction);
            return entities?.ToModels() ?? Enumerable.Empty<Authorization>();
        }

        public async Task<int> InsertAsync(Authorization authorization)
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            var entity = authorization.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        public async Task<bool> UpdateAsync(Authorization authorization)
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            var entity = authorization.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _context.Connection.DeleteAsync(new AuthorizationEntity { Id = id }, _context.Transaction);
        }

        public async Task<long> CountAsync()
        {
            var sql = "SELECT COUNT(*) FROM openiddict_authorizations";
            return await _context.Connection.ExecuteScalarAsync<long>(sql, transaction: _context.Transaction);
        }
    }
}
