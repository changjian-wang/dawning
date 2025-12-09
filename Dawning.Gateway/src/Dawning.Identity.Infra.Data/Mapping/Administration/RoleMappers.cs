using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using System.Collections.Generic;
using System.Linq;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// 角色映射扩展方法
    /// </summary>
    public static class RoleMappers
    {
        /// <summary>
        /// 将实体转换为领域模型
        /// </summary>
        public static Role ToModel(this RoleEntity entity)
        {
            return new Role
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                IsSystem = entity.IsSystem,
                IsActive = entity.IsActive,
                Permissions = entity.Permissions,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                DeletedAt = entity.DeletedAt,
                Timestamp = entity.Timestamp
            };
        }

        /// <summary>
        /// 将领域模型转换为实体
        /// </summary>
        public static RoleEntity ToEntity(this Role model)
        {
            return new RoleEntity
            {
                Id = model.Id,
                Name = model.Name,
                DisplayName = model.DisplayName,
                Description = model.Description,
                IsSystem = model.IsSystem,
                IsActive = model.IsActive,
                Permissions = model.Permissions,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedAt = model.UpdatedAt,
                UpdatedBy = model.UpdatedBy,
                DeletedAt = model.DeletedAt,
                Timestamp = model.Timestamp
            };
        }

        /// <summary>
        /// 将实体集合转换为领域模型集合
        /// </summary>
        public static IEnumerable<Role> ToModels(this IEnumerable<RoleEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// 将领域模型集合转换为实体集合
        /// </summary>
        public static IEnumerable<RoleEntity> ToEntities(this IEnumerable<Role> models)
        {
            return models.Select(m => m.ToEntity());
        }
    }
}
