using Dapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;
using Dawning.Shared.Dapper.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

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
            _context = context;
        }

        /// <summary>
        /// 根据ID异步获取Token
        /// </summary>
        public async Task<Token> GetAsync(Guid id)
        {
            TokenEntity entity = await _context.Connection.GetAsync<TokenEntity>(id, _context.Transaction);
            return entity?.ToModel() ?? new Token();
        }

        /// <summary>
        /// 根据ReferenceId获取Token
        /// </summary>
        public async Task<Token?> GetByReferenceIdAsync(string referenceId)
        {
            var result = await _context.Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(referenceId), t => t.ReferenceId == referenceId)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// 根据Subject获取Token列表
        /// </summary>
        public async Task<IEnumerable<Token>> GetBySubjectAsync(string subject)
        {
            var result = await _context.Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(subject), t => t.Subject == subject)
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// 根据ApplicationId获取Token列表
        /// </summary>
        public async Task<IEnumerable<Token>> GetByApplicationIdAsync(Guid applicationId)
        {
            var result = await _context.Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(applicationId != Guid.Empty, t => t.ApplicationId == applicationId)
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// 根据AuthorizationId获取Token列表
        /// </summary>
        public async Task<IEnumerable<Token>> GetByAuthorizationIdAsync(Guid authorizationId)
        {
            var result = await _context.Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(authorizationId != Guid.Empty, t => t.AuthorizationId == authorizationId)
                .AsListAsync();

            return result?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        public async Task<PagedData<Token>> GetPagedListAsync(TokenModel model, int page, int itemsPerPage)
        {
            PagedResult<TokenEntity> result = await _context.Connection.Builder<TokenEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Subject), t => t.Subject!.Contains(model.Subject ?? ""))
                .WhereIf(model.ApplicationId.HasValue, t => t.ApplicationId == model.ApplicationId)
                .WhereIf(model.AuthorizationId.HasValue, t => t.AuthorizationId == model.AuthorizationId)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Type), t => t.Type == model.Type)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Status), t => t.Status == model.Status)
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<Token> pagedData = new PagedData<Token>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels()
            };

            return pagedData;
        }

        /// <summary>
        /// 获取所有Token
        /// </summary>
        public async Task<IEnumerable<Token>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<TokenEntity>(_context.Transaction);
            return list?.ToModels() ?? new List<Token>();
        }

        /// <summary>
        /// 异步插入Token
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
        /// 异步更新Token
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
        /// 异步删除Token
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
        /// 清理过期的令牌
        /// </summary>
        public async Task<int> PruneExpiredTokensAsync()
        {
            var expiredTokens = await _context.Connection.Builder<TokenEntity>(_context.Transaction)
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
    }
}
