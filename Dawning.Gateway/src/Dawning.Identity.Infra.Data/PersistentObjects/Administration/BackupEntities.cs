using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 备份记录实体
    /// </summary>
    [Table("backup_records")]
    public class BackupRecordEntity
    {
        /// <summary>
        /// 备份ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 备份文件名
        /// </summary>
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 备份文件路径
        /// </summary>
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        [Column("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// 备份类型
        /// </summary>
        [Column("backup_type")]
        public string BackupType { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 备份说明
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否为手动触发
        /// </summary>
        [Column("is_manual")]
        public bool IsManual { get; set; }

        /// <summary>
        /// 备份状态（success, failed, pending）
        /// </summary>
        [Column("status")]
        public string Status { get; set; } = "success";

        /// <summary>
        /// 错误消息
        /// </summary>
        [Column("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
