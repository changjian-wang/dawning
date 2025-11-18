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
    /// Token Repository 实现
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly DbContext _context;

        public TokenRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Token?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<TokenEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        public async Task<Token?> GetByReferenceIdAsync(string referenceId)
        {
            var sql = "SELECT * FROM openiddict_tokens WHERE reference_id = @ReferenceId";
            var entity = await _context.Connection.QueryFirstOrDefaultAsync<TokenEntity>(
                sql,
                new { ReferenceId = referenceId },
                _context.Transaction
            );
            return entity?.ToModel();
        }

        public async Task<IEnumerable<Token>> GetBySubjectAsync(string subject)
        {
            var sql = "SELECT * FROM openiddict_tokens WHERE subject = @Subject";
            var entities = await _context.Connection.QueryAsync<TokenEntity>(
                sql,
                new { Subject = subject },
                _context.Transaction
            );
            return entities?.ToModels() ?? Enumerable.Empty<Token>();
        }

        public async Task<IEnumerable<Token>> GetByApplicationIdAsync(Guid applicationId)
        {
            var sql = "SELECT * FROM openiddict_tokens WHERE application_id = @ApplicationId";
            var entities = await _context.Connection.QueryAsync<TokenEntity>(
                sql,
                new { ApplicationId = applicationId },
                _context.Transaction
            );
            return entities?.ToModels() ?? Enumerable.Empty<Token>();
        }

        public async Task<IEnumerable<Token>> GetByAuthorizationIdAsync(Guid authorizationId)
        {
            var sql = "SELECT * FROM openiddict_tokens WHERE authorization_id = @AuthorizationId";
            var entities = await _context.Connection.QueryAsync<TokenEntity>(
                sql,
                new { AuthorizationId = authorizationId },
                _context.Transaction
            );
            return entities?.ToModels() ?? Enumerable.Empty<Token>();
        }

        public async Task<IEnumerable<Token>> GetAllAsync()
        {
            var entities = await _context.Connection.GetAllAsync<TokenEntity>(_context.Transaction);
            return entities?.ToModels() ?? Enumerable.Empty<Token>();
        }

        public async Task<int> InsertAsync(Token token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var entity = token.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        public async Task<bool> UpdateAsync(Token token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var entity = token.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _context.Connection.DeleteAsync(new TokenEntity { Id = id }, _context.Transaction);
        }

        public async Task<long> CountAsync()
        {
            var sql = "SELECT COUNT(*) FROM openiddict_tokens";
            return await _context.Connection.ExecuteScalarAsync<long>(sql, transaction: _context.Transaction);
        }

        /// <summary>
        /// 清理过期的令牌
        /// </summary>
        public async Task<int> PruneExpiredTokensAsync()
        {
            var sql = @"
                DELETE FROM openiddict_tokens 
                WHERE expires_at IS NOT NULL 
                AND expires_at < @Now
                AND status != 'valid'";

            return await _context.Connection.ExecuteAsync(
                sql,
                new { Now = DateTime.UtcNow },
                _context.Transaction
            );
        }
    }
}
