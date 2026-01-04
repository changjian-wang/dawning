using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.ORM.Dapper;
using static Dawning.ORM.Dapper.SqlMapperExtensions;

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
            var entity = await _context.Connection.GetAsync<PermissionEntity>(
                id,
                _context.Transaction
            );
            return entity?.ToDomain();
        }

        public async Task<Permission?> GetByCodeAsync(string code)
        {
            var entity = await _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.Code == code)
                .FirstOrDefaultAsync();

            return entity?.ToDomain();
        }

        public async Task<PagedData<Permission>> GetPagedListAsync(
            PermissionModel model,
            int page,
            int pageSize
        )
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
                PageSize = pageSize,
            };
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            var entities = await _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return new List<Permission>();

            var entities = await _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId)
        {
            // First get role permission associations
            var rolePermissions = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            if (!rolePermissions.Any())
                return new List<Permission>();

            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

            // Get permission details
            var entities = await _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive == true)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();

            return entities.ToDomains() ?? new List<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource)
        {
            var entities = await _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
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
            var builder = _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
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
            var entities = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            return entities.Select(e => e.ToDomain());
        }

        public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId)
        {
            var entities = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.PermissionId == permissionId)
                .ToListAsync();

            return entities.Select(e => e.ToDomain());
        }

        public async Task<bool> AddRolePermissionsAsync(
            Guid roleId,
            IEnumerable<Guid> permissionIds,
            Guid? operatorId = null
        )
        {
            var permissionIdList = permissionIds.ToList();
            if (!permissionIdList.Any())
                return true;

            var now = DateTime.UtcNow;

            // Get existing permission associations
            var existingEntities = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
            var existingPermissionIds = existingEntities.Select(e => e.PermissionId).ToHashSet();

            // Only insert associations that don't exist
            var newPermissionIds = permissionIdList
                .Where(id => !existingPermissionIds.Contains(id))
                .ToList();

            foreach (var permissionId in newPermissionIds)
            {
                var entity = new RolePermissionEntity
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = now,
                    CreatedBy = operatorId,
                };
                await _context.Connection.InsertAsync(entity, _context.Transaction);
            }

            return true;
        }

        public async Task<bool> RemoveRolePermissionsAsync(
            Guid roleId,
            IEnumerable<Guid> permissionIds
        )
        {
            var permissionIdList = permissionIds.ToList();
            if (!permissionIdList.Any())
                return true;

            // Find entities to delete
            var entitiesToDelete = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.RoleId == roleId && permissionIdList.Contains(rp.PermissionId))
                .ToListAsync();

            foreach (var entity in entitiesToDelete)
            {
                await _context.Connection.DeleteAsync(entity, _context.Transaction);
            }

            return true;
        }

        public async Task<bool> RemoveAllRolePermissionsAsync(Guid roleId)
        {
            var entities = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            foreach (var entity in entities)
            {
                await _context.Connection.DeleteAsync(entity, _context.Transaction);
            }

            return true;
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionCode)
        {
            // First get the permission
            var permission = await _context
                .Connection.Builder<PermissionEntity>(_context.Transaction)
                .Where(p => p.Code == permissionCode)
                .FirstOrDefaultAsync();

            if (permission == null)
                return false;

            // Check if the role has this permission
            var rolePermission = await _context
                .Connection.Builder<RolePermissionEntity>(_context.Transaction)
                .Where(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id)
                .FirstOrDefaultAsync();

            return rolePermission != null;
        }
    }
}
