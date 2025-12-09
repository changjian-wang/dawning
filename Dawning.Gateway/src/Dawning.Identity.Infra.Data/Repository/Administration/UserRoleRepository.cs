using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

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
            const string sql = @"
                SELECT r.* 
                FROM roles r
                INNER JOIN user_roles ur ON r.id = ur.role_id
                WHERE ur.user_id = @UserId 
                  AND r.deleted_at IS NULL
                ORDER BY r.name";

            var entities = await _context.Connection.QueryAsync<RoleEntity>(
                sql,
                new { UserId = userId },
                _context.Transaction);

            return entities.ToModels();
        }

        /// <summary>
        /// 获取拥有指定角色的所有用户
        /// </summary>
        public async Task<IEnumerable<User>> GetRoleUsersAsync(Guid roleId)
        {
            const string sql = @"
                SELECT u.* 
                FROM users u
                INNER JOIN user_roles ur ON u.id = ur.user_id
                WHERE ur.role_id = @RoleId
                ORDER BY u.username";

            var entities = await _context.Connection.QueryAsync<UserEntity>(
                sql,
                new { RoleId = roleId },
                _context.Transaction);

            return entities.ToModels();
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
                CreatedBy = operatorId
            };

            var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
            return result > 0;
        }

        /// <summary>
        /// 批量为用户分配角色
        /// </summary>
        public async Task<bool> AssignRolesAsync(Guid userId, IEnumerable<Guid> roleIds, Guid? operatorId = null)
        {
            if (!roleIds.Any()) return true;

            // 先移除所有现有角色
            await RemoveAllRolesAsync(userId);

            // 批量插入新角色
            var entities = roleIds.Select(roleId => new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId
            }).ToList();

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
            const string sql = @"
                DELETE FROM user_roles 
                WHERE user_id = @UserId AND role_id = @RoleId";

            var affected = await _context.Connection.ExecuteAsync(
                sql,
                new { UserId = userId, RoleId = roleId },
                _context.Transaction);

            return affected > 0;
        }

        /// <summary>
        /// 移除用户的所有角色
        /// </summary>
        public async Task<bool> RemoveAllRolesAsync(Guid userId)
        {
            const string sql = "DELETE FROM user_roles WHERE user_id = @UserId";

            await _context.Connection.ExecuteAsync(
                sql,
                new { UserId = userId },
                _context.Transaction);

            return true;
        }

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        public async Task<bool> HasRoleAsync(Guid userId, Guid roleId)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM user_roles 
                WHERE user_id = @UserId AND role_id = @RoleId";

            var count = await _context.Connection.ExecuteScalarAsync<int>(
                sql,
                new { UserId = userId, RoleId = roleId },
                _context.Transaction);

            return count > 0;
        }

        /// <summary>
        /// 检查用户是否拥有指定角色（按角色名称）
        /// </summary>
        public async Task<bool> HasRoleByNameAsync(Guid userId, string roleName)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM user_roles ur
                INNER JOIN roles r ON ur.role_id = r.id
                WHERE ur.user_id = @UserId 
                  AND r.name = @RoleName
                  AND r.deleted_at IS NULL";

            var count = await _context.Connection.ExecuteScalarAsync<int>(
                sql,
                new { UserId = userId, RoleName = roleName },
                _context.Transaction);

            return count > 0;
        }
    }
}
