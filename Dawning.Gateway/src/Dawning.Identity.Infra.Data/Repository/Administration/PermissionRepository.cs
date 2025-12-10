using Dapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly DbContext _context;

        public PermissionRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetAsync(Guid id)
        {
            var entity = await _context.Connection.GetAsync<PermissionEntity>(id, _context.Transaction);
            return entity?.ToDomain();
        }

        public async Task<Permission?> GetByCodeAsync(string code)
        {
            var entity = await _context.Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.Code == code)
                .FirstOrDefaultAsync();

            return entity?.ToDomain();
        }

        public async Task<PagedData<Permission>> GetPagedListAsync(PermissionModel model, int page, int pageSize)
        {
            var builder = _context.Connection.Builder<PermissionEntity>(_context.Transaction);

            if (!string.IsNullOrWhiteSpace(model.Code))
                builder.Where(p => p.Code!.Contains(model.Code));

            if (!string.IsNullOrWhiteSpace(model.Name))
                builder.Where(p => p.Name!.Contains(model.Name));

            if (!string.IsNullOrWhiteSpace(model.Resource))
                builder.Where(p => p.Resource == model.Resource);

            if (!string.IsNullOrWhiteSpace(model.Action))
                builder.Where(p => p.Action == model.Action);

            if (!string.IsNullOrWhiteSpace(model.Category))
                builder.Where(p => p.Category == model.Category);

            if (model.IsActive.HasValue)
                builder.Where(p => p.IsActive == model.IsActive.Value);

            var result = await builder
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Code)
                .AsPagedListAsync(page, pageSize);

            return new PagedData<Permission>
            {
                Items = result.Values.ToDomains() ?? new List<Permission>(),
                TotalCount = result.TotalItems,
                PageIndex = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            var entities = await _context.Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any()) return new List<Permission>();

            var entities = await _context.Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId)
        {
            var query = @"
                SELECT p.* FROM permissions p
                INNER JOIN role_permissions rp ON p.id = rp.permission_id
                WHERE rp.role_id = @RoleId AND p.is_active = true
                ORDER BY p.display_order, p.code";

            var entities = await _context.Connection
                .QueryAsync<PermissionEntity>(query, new { RoleId = roleId }, _context.Transaction);

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource)
        {
            var entities = await _context.Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.Resource == resource && p.IsActive == true)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<bool> InsertAsync(Permission permission)
        {
            var entity = permission.ToEntity();
            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result > 0;
        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            var entity = permission.ToEntity();
            var result = await _context.Connection.UpdateAsync(entity, _context.Transaction);
            return result;
        }

        public async Task<bool> DeleteAsync(Permission permission)
        {
            var entity = permission.ToEntity();
            var result = await _context.Connection.DeleteAsync(entity, _context.Transaction);
            return result;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var builder = _context.Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.Code == code);

            if (excludeId.HasValue)
                builder.Where(p => p.Id != excludeId.Value);

            var entity = await builder.FirstOrDefaultAsync();
            return entity != null;
        }
    }

    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly DbContext _context;

        public RolePermissionRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId)
        {
            var query = "SELECT * FROM role_permissions WHERE role_id = @RoleId";
            var entities = await _context.Connection
                .QueryAsync<RolePermissionEntity>(query, new { RoleId = roleId }, _context.Transaction);

            return entities.Select(e => e.ToDomain());
        }

        public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId)
        {
            var query = "SELECT * FROM role_permissions WHERE permission_id = @PermissionId";
            var entities = await _context.Connection
                .QueryAsync<RolePermissionEntity>(query, new { PermissionId = permissionId }, _context.Transaction);

            return entities.Select(e => e.ToDomain());
        }

        public async Task<bool> AddRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid? operatorId = null)
        {
            var now = DateTime.UtcNow;
            var rolePermissions = permissionIds.Select(permissionId => new
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = now,
                CreatedBy = operatorId
            }).ToList();

            var query = @"
                INSERT INTO role_permissions (id, role_id, permission_id, created_at, created_by)
                VALUES (@Id, @RoleId, @PermissionId, @CreatedAt, @CreatedBy)
                ON CONFLICT (role_id, permission_id) DO NOTHING";

            var result = await _context.Connection
                .ExecuteAsync(query, rolePermissions, _context.Transaction);

            return result > 0;
        }

        public async Task<bool> RemoveRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
        {
            var query = "DELETE FROM role_permissions WHERE role_id = @RoleId AND permission_id = ANY(@PermissionIds)";
            var result = await _context.Connection
                .ExecuteAsync(query, new { RoleId = roleId, PermissionIds = permissionIds.ToArray() }, _context.Transaction);

            return result > 0;
        }

        public async Task<bool> RemoveAllRolePermissionsAsync(Guid roleId)
        {
            var query = "DELETE FROM role_permissions WHERE role_id = @RoleId";
            var result = await _context.Connection
                .ExecuteAsync(query, new { RoleId = roleId }, _context.Transaction);

            return result > 0;
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionCode)
        {
            var query = @"
                SELECT COUNT(1) FROM role_permissions rp
                INNER JOIN permissions p ON rp.permission_id = p.id
                WHERE rp.role_id = @RoleId AND p.code = @PermissionCode";

            var count = await _context.Connection
                .ExecuteScalarAsync<int>(query, new { RoleId = roleId, PermissionCode = permissionCode }, _context.Transaction);

            return count > 0;
        }
    }
}
