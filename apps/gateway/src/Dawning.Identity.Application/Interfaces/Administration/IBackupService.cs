using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// Database backup service interface
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// Create database backup
        /// </summary>
        /// <param name="options">Backup options</param>
        /// <returns>Backup result</returns>
        Task<BackupResult> CreateBackupAsync(BackupOptions? options = null);

        /// <summary>
        /// Get backup history list
        /// </summary>
        /// <param name="count">Number of records to return</param>
        /// <returns>List of backup records</returns>
        Task<List<BackupRecord>> GetBackupHistoryAsync(int count = 20);

        /// <summary>
        /// Delete specified backup
        /// </summary>
        /// <param name="backupId">Backup ID</param>
        /// <returns>Whether successful</returns>
        Task<bool> DeleteBackupAsync(Guid backupId);

        /// <summary>
        /// Clean up expired backups
        /// </summary>
        /// <param name="retentionDays">Retention days</param>
        /// <returns>Number of deleted backups</returns>
        Task<int> CleanupOldBackupsAsync(int retentionDays);

        /// <summary>
        /// Get backup configuration
        /// </summary>
        /// <returns>Backup configuration</returns>
        Task<BackupConfiguration> GetConfigurationAsync();

        /// <summary>
        /// Update backup configuration
        /// </summary>
        /// <param name="config">New configuration</param>
        Task UpdateConfigurationAsync(BackupConfiguration config);
    }

    /// <summary>
    /// Backup options
    /// </summary>
    public class BackupOptions
    {
        /// <summary>
        /// Backup type (full=full backup, incremental=incremental backup)
        /// </summary>
        public string BackupType { get; set; } = "full";

        /// <summary>
        /// Whether to compress backup file
        /// </summary>
        public bool Compress { get; set; } = true;

        /// <summary>
        /// Whether to include configuration tables
        /// </summary>
        public bool IncludeConfigs { get; set; } = true;

        /// <summary>
        /// Whether to include log tables
        /// </summary>
        public bool IncludeLogs { get; set; } = false;

        /// <summary>
        /// Backup description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether manually triggered
        /// </summary>
        public bool IsManual { get; set; } = true;
    }

    /// <summary>
    /// Backup result
    /// </summary>
    public class BackupResult
    {
        /// <summary>
        /// Whether successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Backup record
        /// </summary>
        public BackupRecord? Backup { get; set; }

        /// <summary>
        /// Error message (if failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Backup duration (seconds)
        /// </summary>
        public double DurationSeconds { get; set; }
    }

    /// <summary>
    /// Backup configuration
    /// </summary>
    public class BackupConfiguration
    {
        /// <summary>
        /// Whether auto backup is enabled
        /// </summary>
        public bool AutoBackupEnabled { get; set; } = true;

        /// <summary>
        /// Auto backup interval (hours)
        /// </summary>
        public int AutoBackupIntervalHours { get; set; } = 24;

        /// <summary>
        /// Auto backup Cron expression
        /// </summary>
        public string AutoBackupCron { get; set; } = "0 2 * * *";

        /// <summary>
        /// Backup retention days
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// Maximum backup count
        /// </summary>
        public int MaxBackupCount { get; set; } = 50;

        /// <summary>
        /// Backup storage path
        /// </summary>
        public string BackupPath { get; set; } = "backups";

        /// <summary>
        /// Whether to compress backups
        /// </summary>
        public bool CompressBackups { get; set; } = true;

        /// <summary>
        /// Auto backup includes logs
        /// </summary>
        public bool AutoBackupIncludeLogs { get; set; } = false;

        /// <summary>
        /// Last auto backup time
        /// </summary>
        public DateTime? LastAutoBackupAt { get; set; }
    }
}
