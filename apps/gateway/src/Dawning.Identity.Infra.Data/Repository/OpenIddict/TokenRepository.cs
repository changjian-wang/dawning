using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Token Repository implementation
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly DbContext _context;

        public TokenRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get Token by ID asynchronously
        /// </summary>
        public async Task<Token> GetAsync(Guid id)
        {
            TokenEntity entity = await _context.Connection.GetAsync<TokenEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel() ?? new Token();
        }

        /// <summary>
        /// Get Token by ReferenceId
        /// </summary>
        public async Task<Token?> GetByReferenceIdAsync(string referenceId)
        {
            var result = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(referenceId), t => t.ReferenceId == referenceId)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// Get Token list by Subject
        /// </summary>
        public async Task<IEnumerable<Token>> GetBySubjectAsync(string subject)
        {
            var result = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(subject), t => t.Subject == subject)
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// Get Token list by ApplicationId
        /// </summary>
        public async Task<IEnumerable<Token>> GetByApplicationIdAsync(Guid applicationId)
        {
            var result = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(applicationId != Guid.Empty, t => t.ApplicationId == applicationId)
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// Get Token list by AuthorizationId
        /// </summary>
        public async Task<IEnumerable<Token>> GetByAuthorizationIdAsync(Guid authorizationId)
        {
            var result = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(authorizationId != Guid.Empty, t => t.AuthorizationId == authorizationId)
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// Get paged list
        /// </summary>
        public async Task<PagedData<Token>> GetPagedListAsync(
            TokenModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedResult<TokenEntity> result = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Subject),
                    t => t.Subject!.Contains(model.Subject ?? "")
                )
                .WhereIf(model.ApplicationId.HasValue, t => t.ApplicationId == model.ApplicationId)
                .WhereIf(
                    model.AuthorizationId.HasValue,
                    t => t.AuthorizationId == model.AuthorizationId
                )
                .WhereIf(!string.IsNullOrWhiteSpace(model.Type), t => t.Type == model.Type)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Status), t => t.Status == model.Status)
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<Token> pagedData = new PagedData<Token>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };

            return pagedData;
        }

        /// <summary>
        /// Get all Tokens
        /// </summary>
        public async Task<IEnumerable<Token>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<TokenEntity>(_context.Transaction);
            return list?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// Insert Token asynchronously
        /// </summary>
        public async ValueTask<int> InsertAsync(Token model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            TokenEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Update Token asynchronously
        /// </summary>
        public async ValueTask<bool> UpdateAsync(Token model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            TokenEntity entity = model.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Delete Token asynchronously
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Token model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            TokenEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Prune expired tokens
        /// </summary>
        public async Task<int> PruneExpiredTokensAsync()
        {
            var expiredTokens = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(true, t => t.ExpiresAt != null && t.ExpiresAt < DateTime.UtcNow)
                .WhereIf(true, t => t.Status != "valid")
                .AsListAsync();

            int deletedCount = 0;
            foreach (var token in expiredTokens)
            {
                if (await _context.Connection.DeleteAsync(token, _context.Transaction))
                {
                    deletedCount++;
                }
            }

            return deletedCount;
        }

        /// <summary>
        /// Get valid tokens list for a user
        /// </summary>
        public async Task<IEnumerable<Token>> GetValidTokensBySubjectAsync(string subject)
        {
            var result = await _context
                .Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(subject), t => t.Subject == subject)
                .WhereIf(true, t => t.Status == "valid")
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// Revoke all valid tokens for a user
        /// </summary>
        public async Task<int> RevokeAllBySubjectAsync(string subject)
        {
            var validTokens = await GetValidTokensBySubjectAsync(subject);
            var tokenList = validTokens.ToList();

            if (tokenList.Count == 0)
            {
                return 0;
            }

            int revokedCount = 0;
            foreach (var token in tokenList)
            {
                token.Status = "revoked";
                if (await UpdateAsync(token))
                {
                    revokedCount++;
                }
            }

            return revokedCount;
        }

        /// <summary>
        /// Revoke a specific token
        /// </summary>
        public async Task<bool> RevokeByIdAsync(Guid tokenId)
        {
            var token = await GetAsync(tokenId);
            if (token == null || token.Id == Guid.Empty || token.Status != "valid")
            {
                return false;
            }

            token.Status = "revoked";
            return await UpdateAsync(token);
        }
    }
}
