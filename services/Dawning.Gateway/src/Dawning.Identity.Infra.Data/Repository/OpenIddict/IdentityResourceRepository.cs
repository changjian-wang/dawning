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
using Dawning.Shared.Dapper.Contrib;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.OpenIddict
{
    /// <summary>
    /// 身份资源仓储实现
    /// </summary>
    public class IdentityResourceRepository : IIdentityResourceRepository
    {
        private readonly DbContext _context;

        public IdentityResourceRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID获取身份资源
        /// </summary>
        public async Task<IdentityResource?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<IdentityResourceEntity>(
                id,
                _context.Transaction
            );
            if (entity == null)
                return null;

            // Load claims
            var claims = await _context
                .Connection.Builder<IdentityResourceClaimEntity>(_context.Transaction)
                .Where(c => c.IdentityResourceId == id)
                .AsListAsync();

            return entity.ToModel(claims);
        }

        /// <summary>
        /// 根据名称获取身份资源
        /// </summary>
        public async Task<IdentityResource?> GetByNameAsync(string name)
        {
            var result = await _context
                .Connection.Builder<IdentityResourceEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(name), r => r.Name == name)
                .AsListAsync();

            var entity = result.FirstOrDefault();
            if (entity == null)
                return null;

            // Load claims
            var claims = await _context
                .Connection.Builder<IdentityResourceClaimEntity>(_context.Transaction)
                .Where(c => c.IdentityResourceId == entity.Id)
                .AsListAsync();

            return entity.ToModel(claims);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        public async Task<PagedData<IdentityResource>> GetPagedListAsync(
            IdentityResourceModel model,
            int page,
            int itemsPerPage
        )
        {
            var result = await _context
                .Connection.Builder<IdentityResourceEntity>(_context.Transaction)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    r => r.Name!.Contains(model.Name ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.DisplayName),
                    r => r.DisplayName!.Contains(model.DisplayName ?? "")
                )
                .WhereIf(model.Enabled.HasValue, r => r.Enabled == model.Enabled!.Value)
                .WhereIf(model.Required.HasValue, r => r.Required == model.Required!.Value)
                .AsPagedListAsync(page, itemsPerPage);

            var resources = new List<IdentityResource>();
            foreach (var entity in result.Values)
            {
                // Load claims
                var claims = await _context
                    .Connection.Builder<IdentityResourceClaimEntity>(_context.Transaction)
                    .Where(c => c.IdentityResourceId == entity.Id)
                    .AsListAsync();

                resources.Add(entity.ToModel(claims));
            }

            return new PagedData<IdentityResource>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = resources,
            };
        }

        /// <summary>
        /// 获取所有身份资源
        /// </summary>
        public async Task<IEnumerable<IdentityResource>> GetAllAsync()
        {
            var entities = await _context.Connection.GetAllAsync<IdentityResourceEntity>(
                _context.Transaction
            );
            var resources = new List<IdentityResource>();

            foreach (var entity in entities ?? Enumerable.Empty<IdentityResourceEntity>())
            {
                // Load claims
                var claims = await _context
                    .Connection.Builder<IdentityResourceClaimEntity>(_context.Transaction)
                    .Where(c => c.IdentityResourceId == entity.Id)
                    .AsListAsync();

                resources.Add(entity.ToModel(claims));
            }

            return resources;
        }

        /// <summary>
        /// 根据名称列表获取身份资源
        /// </summary>
        public async Task<IEnumerable<IdentityResource>> GetByNamesAsync(IEnumerable<string> names)
        {
            if (names == null || !names.Any())
                return Enumerable.Empty<IdentityResource>();

            var nameList = names.ToList();
            var sql = $"SELECT * FROM identity_resources WHERE name IN @Names";
            var entities = await _context.Connection.QueryAsync<IdentityResourceEntity>(
                sql,
                new { Names = nameList },
                _context.Transaction
            );

            var resources = new List<IdentityResource>();
            foreach (var entity in entities)
            {
                // Load claims
                var claims = await _context
                    .Connection.Builder<IdentityResourceClaimEntity>(_context.Transaction)
                    .Where(c => c.IdentityResourceId == entity.Id)
                    .AsListAsync();

                resources.Add(entity.ToModel(claims));
            }

            return resources;
        }

        /// <summary>
        /// 插入身份资源
        /// </summary>
        public async ValueTask<int> InsertAsync(IdentityResource model)
        {
            var entity = model.ToEntity();
            await _context.Connection.InsertAsync(entity, _context.Transaction);

            // Insert claims
            if (model.UserClaims != null && model.UserClaims.Any())
            {
                foreach (var claim in model.UserClaims)
                {
                    var claimEntity = new IdentityResourceClaimEntity
                    {
                        Id = Guid.NewGuid(),
                        IdentityResourceId = entity.Id,
                        Type = claim,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Connection.InsertAsync(claimEntity, _context.Transaction);
                }
            }

            return 1;
        }

        /// <summary>
        /// 更新身份资源
        /// </summary>
        public async ValueTask<bool> UpdateAsync(IdentityResource model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);

            // Delete existing claims
            await _context.Connection.ExecuteAsync(
                "DELETE FROM identity_resource_claims WHERE identity_resource_id = @Id",
                new { Id = entity.Id },
                _context.Transaction
            );

            // Insert new claims
            if (model.UserClaims != null && model.UserClaims.Any())
            {
                foreach (var claim in model.UserClaims)
                {
                    var claimEntity = new IdentityResourceClaimEntity
                    {
                        Id = Guid.NewGuid(),
                        IdentityResourceId = entity.Id,
                        Type = claim,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Connection.InsertAsync(claimEntity, _context.Transaction);
                }
            }

            return result;
        }

        /// <summary>
        /// 删除身份资源
        /// </summary>
        public async ValueTask<bool> DeleteAsync(IdentityResource model)
        {
            var entity = model.ToEntity();

            // Delete claims (handled by foreign key cascade)
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }
    }
}
