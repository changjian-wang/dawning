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
    /// Authorization Repository implementation
    /// </summary>
    public class AuthorizationRepository : IAuthorizationRepository
    {
        private readonly DbContext _context;

        public AuthorizationRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get Authorization by ID asynchronously
        /// </summary>
        public async Task<Authorization> GetAsync(Guid id)
        {
            AuthorizationEntity entity = await _context.Connection.GetAsync<AuthorizationEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToModel() ?? new Authorization();
        }

        /// <summary>
        /// Get Authorization list by Subject
        /// </summary>
        public async Task<IEnumerable<Authorization>> GetBySubjectAsync(string subject)
        {
            var result = await _context
                .Connection.Builder<AuthorizationEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(subject), a => a.Subject == subject)
                .AsListAsync();

            return result?.ToModels() ?? new List<Authorization>();
        }

        /// <summary>
        /// Get Authorization list by ApplicationId
        /// </summary>
        public async Task<IEnumerable<Authorization>> GetByApplicationIdAsync(Guid applicationId)
        {
            var result = await _context
                .Connection.Builder<AuthorizationEntity>(_context.Transaction)
                .WhereIf(applicationId != Guid.Empty, a => a.ApplicationId == applicationId)
                .AsListAsync();

            return result?.ToModels() ?? new List<Authorization>();
        }

        /// <summary>
        /// Get paged list
        /// </summary>
        public async Task<PagedData<Authorization>> GetPagedListAsync(
            AuthorizationModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedResult<AuthorizationEntity> result = await _context
                .Connection.Builder<AuthorizationEntity>(_context.Transaction)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Subject),
                    a => a.Subject!.Contains(model.Subject ?? "")
                )
                .WhereIf(model.ApplicationId.HasValue, a => a.ApplicationId == model.ApplicationId)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Status), a => a.Status == model.Status)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Type), a => a.Type == model.Type)
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<Authorization> pagedData = new PagedData<Authorization>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };

            return pagedData;
        }

        /// <summary>
        /// Get all Authorizations
        /// </summary>
        public async Task<IEnumerable<Authorization>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<AuthorizationEntity>(
                _context.Transaction
            );
            return list?.ToModels() ?? new List<Authorization>();
        }

        /// <summary>
        /// Insert Authorization asynchronously
        /// </summary>
        public async ValueTask<int> InsertAsync(Authorization model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            AuthorizationEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Update Authorization asynchronously
        /// </summary>
        public async ValueTask<bool> UpdateAsync(Authorization model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            AuthorizationEntity entity = model.ToEntity();
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Delete Authorization asynchronously
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Authorization model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            AuthorizationEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }
    }
}
