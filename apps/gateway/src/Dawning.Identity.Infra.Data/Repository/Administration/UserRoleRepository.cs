using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// User-role association repository implementation
    /// </summary>
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly DbContext _context;

        public UserRoleRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            // Step 1: Get the role ID list for the user
            var userRoles = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (!userRoles.Any())
            {
                return Enumerable.Empty<Role>();
            }

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // Step 2: Get role details
            var roles = await _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .Where(r => roleIds.Contains(r.Id) && r.DeletedAt == null)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return roles.ToModels();
        }

        /// <summary>
        /// Get all users with the specified role
        /// </summary>
        public async Task<IEnumerable<User>> GetRoleUsersAsync(Guid roleId)
        {
            // Step 1: Get the user ID list that have this role
            var userRoles = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync();

            if (!userRoles.Any())
            {
                return Enumerable.Empty<User>();
            }

            var userIds = userRoles.Select(ur => ur.UserId).ToList();

            // Step 2: Get user details
            var users = await _context
                .Connection.Builder<UserEntity>(_context.Transaction)
                .Where(u => userIds.Contains(u.Id))
                .OrderBy(u => u.Username)
                .ToListAsync();

            return users.ToModels();
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid? operatorId = null)
        {
            // Check if already exists
            if (await HasRoleAsync(userId, roleId))
            {
                return true; // Already exists, return success
            }

            var entity = new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId,
            };

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result > 0;
        }

        /// <summary>
        /// Batch assign roles to user
        /// </summary>
        public async Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        )
        {
            if (!roleIds.Any())
                return true;

            // First remove all existing roles
            await RemoveAllRolesAsync(userId);

            // Batch insert new roles
            var entities = roleIds
                .Select(roleId => new UserRoleEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = roleId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = operatorId,
                })
                .ToList();

            foreach (var entity in entities)
            {
                await _context.Connection.InsertAsync(entity, _context.Transaction);
            }

            return true;
        }

        /// <summary>
        /// Remove role from user
        /// </summary>
        public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
        {
            // Find the record to delete
            var entity = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return false;
            }

            var result = await _context.Connection.DeleteAsync(entity, _context.Transaction);
            return result;
        }

        /// <summary>
        /// Remove all roles from user
        /// </summary>
        public async Task<bool> RemoveAllRolesAsync(Guid userId)
        {
            // Find all role associations for the user
            var entities = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            // Delete one by one
            foreach (var entity in entities)
            {
                await _context.Connection.DeleteAsync(entity, _context.Transaction);
            }

            return true;
        }

        /// <summary>
        /// Check if user has the specified role
        /// </summary>
        public async Task<bool> HasRoleAsync(Guid userId, Guid roleId)
        {
            var entity = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
                .FirstOrDefaultAsync();

            return entity != null;
        }

        /// <summary>
        /// Check if user has the specified role (by role name)
        /// </summary>
        public async Task<bool> HasRoleByNameAsync(Guid userId, string roleName)
        {
            // Step 1: Get the role ID list for the user
            var userRoles = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (!userRoles.Any())
            {
                return false;
            }

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // Step 2: Check if there is a matching role
            var role = await _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .Where(r => roleIds.Contains(r.Id) && r.Name == roleName && r.DeletedAt == null)
                .FirstOrDefaultAsync();

            return role != null;
        }
    }
}
