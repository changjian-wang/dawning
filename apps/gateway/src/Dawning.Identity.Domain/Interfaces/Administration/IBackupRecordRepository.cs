using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// Backup record repository interface
    /// </summary>
    public interface IBackupRecordRepository
    {
        /// <summary>
        /// Get backup history list
        /// </summary>
        /// <param name="count">Number of records to return</param>
        /// <returns>List of backup records</returns>
        Task<List<BackupRecord>> GetHistoryAsync(int count = 20);

        /// <summary>
        /// Get backup record by ID
        /// </summary>
        Task<BackupRecord?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create backup record
        /// </summary>
        Task<bool> CreateAsync(BackupRecord record);

        /// <summary>
        /// Update backup record
        /// </summary>
        Task<bool> UpdateAsync(BackupRecord record);

        /// <summary>
        /// Delete backup record
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Get expired backup records
        /// </summary>
        /// <param name="cutoffDate">Cutoff date</param>
        /// <returns>List of expired backup records</returns>
        Task<List<BackupRecord>> GetExpiredAsync(DateTime cutoffDate);

        /// <summary>
        /// Batch delete backup records
        /// </summary>
        /// <param name="cutoffDate">Cutoff date</param>
        /// <returns>Number of deleted records</returns>
        Task<int> DeleteExpiredAsync(DateTime cutoffDate);
    }
}
