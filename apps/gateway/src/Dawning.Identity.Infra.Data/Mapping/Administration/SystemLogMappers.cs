using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// 系统日志映射扩展方法
    /// </summary>
    public static class SystemLogMappers
    {
        /// <summary>
        /// 将实体转换为领域模型
        /// </summary>
        public static SystemLog ToModel(this SystemLogEntity entity)
        {
            return new SystemLog
            {
                Id = entity.Id,
                Level = entity.Level,
                Message = entity.Message,
                Exception = entity.Exception,
                StackTrace = entity.StackTrace,
                Source = entity.Source,
                UserId = entity.UserId,
                Username = entity.Username,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                RequestPath = entity.RequestPath,
                RequestMethod = entity.RequestMethod,
                StatusCode = entity.StatusCode,
                CreatedAt = entity.CreatedAt,
                Timestamp = entity.Timestamp,
            };
        }

        /// <summary>
        /// 将领域模型转换为实体
        /// </summary>
        public static SystemLogEntity ToEntity(this SystemLog model)
        {
            return new SystemLogEntity
            {
                Id = model.Id,
                Level = model.Level,
                Message = model.Message,
                Exception = model.Exception,
                StackTrace = model.StackTrace,
                Source = model.Source,
                UserId = model.UserId,
                Username = model.Username,
                IpAddress = model.IpAddress,
                UserAgent = model.UserAgent,
                RequestPath = model.RequestPath,
                RequestMethod = model.RequestMethod,
                StatusCode = model.StatusCode,
                CreatedAt = model.CreatedAt,
                Timestamp = model.Timestamp,
            };
        }

        /// <summary>
        /// 将实体集合转换为领域模型集合
        /// </summary>
        public static IEnumerable<SystemLog> ToModels(this IEnumerable<SystemLogEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// 将领域模型集合转换为实体集合
        /// </summary>
        public static IEnumerable<SystemLogEntity> ToEntities(this IEnumerable<SystemLog> models)
        {
            return models.Select(m => m.ToEntity());
        }
    }
}
