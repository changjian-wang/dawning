using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using System.Collections.Generic;
using System.Linq;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    public static class PermissionMappers
    {
        public static Permission ToDomain(this PermissionEntity entity)
        {
            return new Permission
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                Resource = entity.Resource,
                Action = entity.Action,
                Category = entity.Category,
                IsSystem = entity.IsSystem,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                Timestamp = entity.Timestamp
            };
        }

        public static PermissionEntity ToEntity(this Permission model)
        {
            return new PermissionEntity
            {
                Id = model.Id,
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                Resource = model.Resource,
                Action = model.Action,
                Category = model.Category,
                IsSystem = model.IsSystem,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedAt = model.UpdatedAt,
                UpdatedBy = model.UpdatedBy,
                Timestamp = model.Timestamp
            };
        }

        public static List<Permission>? ToDomains(this IEnumerable<PermissionEntity>? entities)
        {
            return entities?.Select(e => e.ToDomain()).ToList();
        }

        public static RolePermission ToDomain(this RolePermissionEntity entity)
        {
            return new RolePermission
            {
                Id = entity.Id,
                RoleId = entity.RoleId,
                PermissionId = entity.PermissionId,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy
            };
        }

        public static RolePermissionEntity ToEntity(this RolePermission model)
        {
            return new RolePermissionEntity
            {
                Id = model.Id,
                RoleId = model.RoleId,
                PermissionId = model.PermissionId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy
            };
        }
    }
}
