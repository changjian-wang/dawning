using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// Backup record entity
    /// </summary>
    [Table("backup_records")]
    public class BackupRecordEntity
    {
        /// <summary>
        /// Backup ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Backup file name
        /// </summary>
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Backup file path
        /// </summary>
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// File size (bytes)
        /// </summary>
        [Column("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Backup type
        /// </summary>
        [Column("backup_type")]
        public string BackupType { get; set; } = string.Empty;

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Backup description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether manually triggered
        /// </summary>
        [Column("is_manual")]
        public bool IsManual { get; set; }

        /// <summary>
        /// Backup status (success, failed, pending)
        /// </summary>
        [Column("status")]
        public string Status { get; set; } = "success";

        /// <summary>
        /// Error message
        /// </summary>
        [Column("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
