using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    /// <summary>
    /// 备份记录仓储接口
    /// </summary>
    public interface IBackupRecordRepository
    {
        /// <summary>
        /// 获取备份历史列表
        /// </summary>
        /// <param name="count">返回数量</param>
        /// <returns>备份记录列表</returns>
        Task<List<BackupRecord>> GetHistoryAsync(int count = 20);

        /// <summary>
        /// 根据ID获取备份记录
        /// </summary>
        Task<BackupRecord?> GetByIdAsync(Guid id);

        /// <summary>
        /// 创建备份记录
        /// </summary>
        Task<bool> CreateAsync(BackupRecord record);

        /// <summary>
        /// 更新备份记录
        /// </summary>
        Task<bool> UpdateAsync(BackupRecord record);

        /// <summary>
        /// 删除备份记录
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 获取过期备份记录
        /// </summary>
        /// <param name="cutoffDate">截止日期</param>
        /// <returns>过期的备份记录列表</returns>
        Task<List<BackupRecord>> GetExpiredAsync(DateTime cutoffDate);

        /// <summary>
        /// 批量删除备份记录
        /// </summary>
        /// <param name="cutoffDate">截止日期</param>
        /// <returns>删除的记录数</returns>
        Task<int> DeleteExpiredAsync(DateTime cutoffDate);
    }
}
