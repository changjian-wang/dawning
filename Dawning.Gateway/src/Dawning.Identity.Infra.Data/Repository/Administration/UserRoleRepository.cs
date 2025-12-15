using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// 用户角色关联仓储实现
    /// </summary>
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly DbContext _context;

        public UserRoleRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取用户的所有角色
        /// </summary>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            // Step 1: 获取用户的角色ID列表
            var userRoles = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (!userRoles.Any())
            {
                return Enumerable.Empty<Role>();
            }

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // Step 2: 获取角色详情
            var roles = await _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .Where(r => roleIds.Contains(r.Id) && r.DeletedAt == null)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return roles.ToModels();
        }

        /// <summary>
        /// 获取拥有指定角色的所有用户
        /// </summary>
        public async Task<IEnumerable<User>> GetRoleUsersAsync(Guid roleId)
        {
            // Step 1: 获取拥有该角色的用户ID列表
            var userRoles = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync();

            if (!userRoles.Any())
            {
                return Enumerable.Empty<User>();
            }

            var userIds = userRoles.Select(ur => ur.UserId).ToList();

            // Step 2: 获取用户详情
            var users = await _context
                .Connection.Builder<UserEntity>(_context.Transaction)
                .Where(u => userIds.Contains(u.Id))
                .OrderBy(u => u.Username)
                .ToListAsync();

            return users.ToModels();
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid? operatorId = null)
        {
            // 检查是否已存在
            if (await HasRoleAsync(userId, roleId))
            {
                return true; // 已存在，返回成功
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
        /// 批量为用户分配角色
        /// </summary>
        public async Task<bool> AssignRolesAsync(
            Guid userId,
            IEnumerable<Guid> roleIds,
            Guid? operatorId = null
        )
        {
            if (!roleIds.Any())
                return true;

            // 先移除所有现有角色
            await RemoveAllRolesAsync(userId);

            // 批量插入新角色
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
        /// 移除用户的角色
        /// </summary>
        public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
        {
            // 查找要删除的记录
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
        /// 移除用户的所有角色
        /// </summary>
        public async Task<bool> RemoveAllRolesAsync(Guid userId)
        {
            // 查找用户的所有角色关联
            var entities = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            // 逐个删除
            foreach (var entity in entities)
            {
                await _context.Connection.DeleteAsync(entity, _context.Transaction);
            }

            return true;
        }

        /// <summary>
        /// 检查用户是否拥有指定角色
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
        /// 检查用户是否拥有指定角色（按角色名称）
        /// </summary>
        public async Task<bool> HasRoleByNameAsync(Guid userId, string roleName)
        {
            // Step 1: 获取用户的角色ID列表
            var userRoles = await _context
                .Connection.Builder<UserRoleEntity>(_context.Transaction)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (!userRoles.Any())
            {
                return false;
            }

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // Step 2: 检查是否有匹配的角色
            var role = await _context
                .Connection.Builder<RoleEntity>(_context.Transaction)
                .Where(r => roleIds.Contains(r.Id) && r.Name == roleName && r.DeletedAt == null)
                .FirstOrDefaultAsync();

            return role != null;
        }
    }
}
