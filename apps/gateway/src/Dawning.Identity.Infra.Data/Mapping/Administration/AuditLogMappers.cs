using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// 审计日志映射扩展方法
    /// </summary>
    public static class AuditLogMappers
    {
        /// <summary>
        /// 将实体转换为领域模型
        /// </summary>
        public static AuditLog ToModel(this AuditLogEntity entity)
        {
            return new AuditLog
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Username = entity.Username,
                Action = entity.Action,
                EntityType = entity.EntityType,
                EntityId = entity.EntityId,
                Description = entity.Description,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                RequestPath = entity.RequestPath,
                RequestMethod = entity.RequestMethod,
                StatusCode = entity.StatusCode,
                OldValues = entity.OldValues,
                NewValues = entity.NewValues,
                CreatedAt = entity.CreatedAt,
                Timestamp = entity.Timestamp,
            };
        }

        /// <summary>
        /// 将领域模型转换为实体
        /// </summary>
        public static AuditLogEntity ToEntity(this AuditLog model)
        {
            return new AuditLogEntity
            {
                Id = model.Id,
                UserId = model.UserId,
                Username = model.Username,
                Action = model.Action,
                EntityType = model.EntityType,
                EntityId = model.EntityId,
                Description = model.Description,
                IpAddress = model.IpAddress,
                UserAgent = model.UserAgent,
                RequestPath = model.RequestPath,
                RequestMethod = model.RequestMethod,
                StatusCode = model.StatusCode,
                OldValues = model.OldValues,
                NewValues = model.NewValues,
                CreatedAt = model.CreatedAt,
                Timestamp = model.Timestamp,
            };
        }

        /// <summary>
        /// 将实体集合转换为领域模型集合
        /// </summary>
        public static IEnumerable<AuditLog> ToModels(this IEnumerable<AuditLogEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// 将领域模型集合转换为实体集合
        /// </summary>
        public static IEnumerable<AuditLogEntity> ToEntities(this IEnumerable<AuditLog> models)
        {
            return models.Select(m => m.ToEntity());
        }
    }
}
