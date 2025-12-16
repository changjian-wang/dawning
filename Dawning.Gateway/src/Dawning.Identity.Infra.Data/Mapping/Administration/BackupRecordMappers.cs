using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// 备份记录实体映射扩展
    /// </summary>
    public static class BackupRecordMappers
    {
        /// <summary>
        /// 实体转模型
        /// </summary>
        public static BackupRecord ToModel(this BackupRecordEntity entity)
        {
            return new BackupRecord
            {
                Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
                FileName = entity.FileName,
                FilePath = entity.FilePath,
                FileSizeBytes = entity.FileSizeBytes,
                BackupType = entity.BackupType,
                CreatedAt = entity.CreatedAt,
                Description = entity.Description,
                IsManual = entity.IsManual,
                Status = entity.Status,
                ErrorMessage = entity.ErrorMessage
            };
        }

        /// <summary>
        /// 模型转实体
        /// </summary>
        public static BackupRecordEntity ToEntity(this BackupRecord model)
        {
            return new BackupRecordEntity
            {
                Id = model.Id.ToString(),
                FileName = model.FileName,
                FilePath = model.FilePath,
                FileSizeBytes = model.FileSizeBytes,
                BackupType = model.BackupType,
                CreatedAt = model.CreatedAt,
                Description = model.Description,
                IsManual = model.IsManual,
                Status = model.Status,
                ErrorMessage = model.ErrorMessage
            };
        }

        /// <summary>
        /// 实体列表转模型列表
        /// </summary>
        public static IEnumerable<BackupRecord> ToModels(this IEnumerable<BackupRecordEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }
    }
}
