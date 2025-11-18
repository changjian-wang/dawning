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
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.OpenIddict
{
    /// <summary>
    /// Application Repository 实现
    /// </summary>
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly DbContext _context;

        public ApplicationRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID异步获取Application
        /// </summary>
        public async Task<Application> GetAsync(Guid id)
        {
            ApplicationEntity entity = await _context.Connection.GetAsync<ApplicationEntity>(id, _context.Transaction);
            return entity?.ToModel() ?? new Application();
        }

        /// <summary>
        /// 根据ClientId获取Application
        /// </summary>
        public async Task<Application?> GetByClientIdAsync(string clientId)
        {
            var result = await _context.Connection.Builder<ApplicationEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(clientId), a => a.ClientId == clientId)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        public async Task<PagedData<Application>> GetPagedListAsync(ApplicationModel model, int page, int itemsPerPage)
        {
            PagedResult<ApplicationEntity> result = await _context.Connection.Builder<ApplicationEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(model.ClientId), a => a.ClientId!.Contains(model.ClientId ?? ""))
                .WhereIf(!string.IsNullOrWhiteSpace(model.DisplayName), a => a.DisplayName!.Contains(model.DisplayName ?? ""))
                .WhereIf(!string.IsNullOrWhiteSpace(model.Type), a => a.Type == model.Type)
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<Application> pagedData = new PagedData<Application>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels()
            };

            return pagedData;
        }

        /// <summary>
        /// 获取所有Application
        /// </summary>
        public async Task<IEnumerable<Application>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<ApplicationEntity>(_context.Transaction);
            return list?.ToModels() ?? new List<Application>();
        }

        /// <summary>
        /// 异步插入Application
        /// </summary>
        public async ValueTask<int> InsertAsync(Application model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ApplicationEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// 异步更新Application
        /// </summary>
        public async ValueTask<bool> UpdateAsync(Application model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ApplicationEntity entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// 异步删除Application
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Application model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ApplicationEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }
    }
}

