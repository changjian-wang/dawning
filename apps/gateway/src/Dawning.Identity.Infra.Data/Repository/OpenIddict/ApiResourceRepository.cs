using System;
using System.Collections.Generic;
using System.Linq;
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
    /// API资源仓储实现
    /// </summary>
    public class ApiResourceRepository : IApiResourceRepository
    {
        private readonly DbContext _context;

        public ApiResourceRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID获取API资源
        /// </summary>
        public async Task<ApiResource?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<ApiResourceEntity>(
                id,
                _context.Transaction
            );
            if (entity == null)
                return null;

            // Load scopes
            var scopes = await _context
                .Connection.Builder<ApiResourceScopeEntity>(_context.Transaction)
                .Where(s => s.ApiResourceId == id)
                .AsListAsync();

            // Load claims
            var claims = await _context
                .Connection.Builder<ApiResourceClaimEntity>(_context.Transaction)
                .Where(c => c.ApiResourceId == id)
                .AsListAsync();

            return entity.ToModel(scopes, claims);
        }

        /// <summary>
        /// 根据名称获取API资源
        /// </summary>
        public async Task<ApiResource?> GetByNameAsync(string name)
        {
            var result = await _context
                .Connection.Builder<ApiResourceEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(name), r => r.Name == name)
                .AsListAsync();

            var entity = result.FirstOrDefault();
            if (entity == null)
                return null;

            // Load scopes
            var scopes = await _context
                .Connection.Builder<ApiResourceScopeEntity>(_context.Transaction)
                .Where(s => s.ApiResourceId == entity.Id)
                .AsListAsync();

            // Load claims
            var claims = await _context
                .Connection.Builder<ApiResourceClaimEntity>(_context.Transaction)
                .Where(c => c.ApiResourceId == entity.Id)
                .AsListAsync();

            return entity.ToModel(scopes, claims);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        public async Task<PagedData<ApiResource>> GetPagedListAsync(
            ApiResourceModel model,
            int page,
            int itemsPerPage
        )
        {
            var result = await _context
                .Connection.Builder<ApiResourceEntity>(_context.Transaction)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    r => r.Name!.Contains(model.Name ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.DisplayName),
                    r => r.DisplayName!.Contains(model.DisplayName ?? "")
                )
                .WhereIf(model.Enabled.HasValue, r => r.Enabled == model.Enabled!.Value)
                .AsPagedListAsync(page, itemsPerPage);

            var resources = new List<ApiResource>();
            foreach (var entity in result.Values)
            {
                // Load scopes
                var scopes = await _context
                    .Connection.Builder<ApiResourceScopeEntity>(_context.Transaction)
                    .Where(s => s.ApiResourceId == entity.Id)
                    .AsListAsync();

                // Load claims
                var claims = await _context
                    .Connection.Builder<ApiResourceClaimEntity>(_context.Transaction)
                    .Where(c => c.ApiResourceId == entity.Id)
                    .AsListAsync();

                resources.Add(entity.ToModel(scopes, claims));
            }

            return new PagedData<ApiResource>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = resources,
            };
        }

        /// <summary>
        /// 获取所有API资源
        /// </summary>
        public async Task<IEnumerable<ApiResource>> GetAllAsync()
        {
            var entities = await _context.Connection.GetAllAsync<ApiResourceEntity>(
                _context.Transaction
            );
            var resources = new List<ApiResource>();

            foreach (var entity in entities ?? Enumerable.Empty<ApiResourceEntity>())
            {
                // Load scopes
                var scopes = await _context
                    .Connection.Builder<ApiResourceScopeEntity>(_context.Transaction)
                    .Where(s => s.ApiResourceId == entity.Id)
                    .AsListAsync();

                // Load claims
                var claims = await _context
                    .Connection.Builder<ApiResourceClaimEntity>(_context.Transaction)
                    .Where(c => c.ApiResourceId == entity.Id)
                    .AsListAsync();

                resources.Add(entity.ToModel(scopes, claims));
            }

            return resources;
        }

        /// <summary>
        /// 根据名称列表获取API资源
        /// </summary>
        public async Task<IEnumerable<ApiResource>> GetByNamesAsync(IEnumerable<string> names)
        {
            if (names == null || !names.Any())
                return Enumerable.Empty<ApiResource>();

            var nameList = names.ToList();
            var sql = $"SELECT * FROM api_resources WHERE name IN @Names";
            var entities = await _context.Connection.QueryAsync<ApiResourceEntity>(
                sql,
                new { Names = nameList },
                _context.Transaction
            );

            var resources = new List<ApiResource>();
            foreach (var entity in entities)
            {
                // Load scopes
                var scopes = await _context
                    .Connection.Builder<ApiResourceScopeEntity>(_context.Transaction)
                    .Where(s => s.ApiResourceId == entity.Id)
                    .AsListAsync();

                // Load claims
                var claims = await _context
                    .Connection.Builder<ApiResourceClaimEntity>(_context.Transaction)
                    .Where(c => c.ApiResourceId == entity.Id)
                    .AsListAsync();

                resources.Add(entity.ToModel(scopes, claims));
            }

            return resources;
        }

        /// <summary>
        /// 插入API资源
        /// </summary>
        public async ValueTask<int> InsertAsync(ApiResource model)
        {
            var entity = model.ToEntity();
            await _context.Connection.InsertAsync(entity, _context.Transaction);

            // Insert scopes
            if (model.Scopes != null && model.Scopes.Any())
            {
                foreach (var scope in model.Scopes)
                {
                    var scopeEntity = new ApiResourceScopeEntity
                    {
                        Id = Guid.NewGuid(),
                        ApiResourceId = entity.Id,
                        Scope = scope,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Connection.InsertAsync(scopeEntity, _context.Transaction);
                }
            }

            // Insert claims
            if (model.UserClaims != null && model.UserClaims.Any())
            {
                foreach (var claim in model.UserClaims)
                {
                    var claimEntity = new ApiResourceClaimEntity
                    {
                        Id = Guid.NewGuid(),
                        ApiResourceId = entity.Id,
                        Type = claim,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Connection.InsertAsync(claimEntity, _context.Transaction);
                }
            }

            return 1;
        }

        /// <summary>
        /// 更新API资源
        /// </summary>
        public async ValueTask<bool> UpdateAsync(ApiResource model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);

            // Delete existing scopes and claims
            await _context.Connection.ExecuteAsync(
                "DELETE FROM api_resource_scopes WHERE api_resource_id = @Id",
                new { Id = entity.Id },
                _context.Transaction
            );

            await _context.Connection.ExecuteAsync(
                "DELETE FROM api_resource_claims WHERE api_resource_id = @Id",
                new { Id = entity.Id },
                _context.Transaction
            );

            // Insert new scopes
            if (model.Scopes != null && model.Scopes.Any())
            {
                foreach (var scope in model.Scopes)
                {
                    var scopeEntity = new ApiResourceScopeEntity
                    {
                        Id = Guid.NewGuid(),
                        ApiResourceId = entity.Id,
                        Scope = scope,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Connection.InsertAsync(scopeEntity, _context.Transaction);
                }
            }

            // Insert new claims
            if (model.UserClaims != null && model.UserClaims.Any())
            {
                foreach (var claim in model.UserClaims)
                {
                    var claimEntity = new ApiResourceClaimEntity
                    {
                        Id = Guid.NewGuid(),
                        ApiResourceId = entity.Id,
                        Type = claim,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Connection.InsertAsync(claimEntity, _context.Transaction);
                }
            }

            return result;
        }

        /// <summary>
        /// 删除API资源
        /// </summary>
        public async ValueTask<bool> DeleteAsync(ApiResource model)
        {
            var entity = model.ToEntity();

            // Delete scopes and claims (handled by foreign key cascade)
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }
    }
}
