using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// 角色仓储实现
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly DbContext _context;

        public RoleRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据ID异步获取角色
        /// </summary>
        public async Task<Role?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<RoleEntity>(id, _context.Transaction);
            return entity?.ToModel();
        }

        /// <summary>
        /// 根据角色名称获取角色
        /// </summary>
        public async Task<Role?> GetByNameAsync(string name)
        {
            var result = await _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .WhereIf(!string.IsNullOrWhiteSpace(name), r => r.Name == name)
                .AsListAsync();

            return result.FirstOrDefault()?.ToModel();
        }

        /// <summary>
        /// 获取分页角色列表
        /// </summary>
        public async Task<PagedData<Role>> GetPagedListAsync(
            RoleModel model,
            int page,
            int itemsPerPage
        )
        {
            var builder = _context.Connection.Builder<RoleEntity>(_context.Transaction);

            // 应用过滤条件
            builder = builder
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    r => r.Name!.Contains(model.Name ?? "")
                )
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.DisplayName),
                    r => r.DisplayName!.Contains(model.DisplayName ?? "")
                )
                .WhereIf(model.IsActive.HasValue, r => r.IsActive == model.IsActive!.Value)
                .WhereIf(model.IsSystem.HasValue, r => r.IsSystem == model.IsSystem!.Value);

            // 按创建时间降序排序
            var result = await builder
                .OrderByDescending(r => r.CreatedAt)
                .AsPagedListAsync(page, itemsPerPage);

            return new PagedData<Role>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var entities = await _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .Where(r => r.DeletedAt == null)
                .OrderBy(r => r.Name)
                .AsListAsync();

            return entities.ToModels();
        }

        /// <summary>
        /// 异步插入角色
        /// </summary>
        public async ValueTask<int> InsertAsync(Role model)
        {
            var entity = model.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 异步更新角色
        /// </summary>
        public async ValueTask<bool> UpdateAsync(Role model)
        {
            var entity = model.ToEntity();
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 异步删除角色（软删除）
        /// </summary>
        public async ValueTask<bool> DeleteAsync(Role model)
        {
            var entity = await _context.Connection.GetAsync<RoleEntity>(
                model.Id,
                _context.Transaction
            );
            if (entity == null)
                return false;

            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = model.UpdatedBy;

            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        public async Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null)
        {
            var builder = _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .Where(r => r.Name == name)
                .Where(r => r.DeletedAt == null);

            if (excludeRoleId.HasValue)
            {
                builder = builder.Where(r => r.Id != excludeRoleId.Value);
            }

            var result = await builder.AsListAsync();
            return result.Any();
        }
    }
}
