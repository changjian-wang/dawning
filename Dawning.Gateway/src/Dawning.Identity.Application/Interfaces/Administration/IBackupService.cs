using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// 数据库备份服务接口
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// 创建数据库备份
        /// </summary>
        /// <param name="options">备份选项</param>
        /// <returns>备份结果</returns>
        Task<BackupResult> CreateBackupAsync(BackupOptions? options = null);

        /// <summary>
        /// 获取备份历史列表
        /// </summary>
        /// <param name="count">返回数量</param>
        /// <returns>备份记录列表</returns>
        Task<List<BackupRecord>> GetBackupHistoryAsync(int count = 20);

        /// <summary>
        /// 删除指定备份
        /// </summary>
        /// <param name="backupId">备份ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteBackupAsync(Guid backupId);

        /// <summary>
        /// 清理过期备份
        /// </summary>
        /// <param name="retentionDays">保留天数</param>
        /// <returns>删除的备份数量</returns>
        Task<int> CleanupOldBackupsAsync(int retentionDays);

        /// <summary>
        /// 获取备份配置
        /// </summary>
        /// <returns>备份配置</returns>
        Task<BackupConfiguration> GetConfigurationAsync();

        /// <summary>
        /// 更新备份配置
        /// </summary>
        /// <param name="config">新配置</param>
        Task UpdateConfigurationAsync(BackupConfiguration config);
    }

    /// <summary>
    /// 备份选项
    /// </summary>
    public class BackupOptions
    {
        /// <summary>
        /// 备份类型（full=完整备份，incremental=增量备份）
        /// </summary>
        public string BackupType { get; set; } = "full";

        /// <summary>
        /// 是否压缩备份文件
        /// </summary>
        public bool Compress { get; set; } = true;

        /// <summary>
        /// 是否包含配置表
        /// </summary>
        public bool IncludeConfigs { get; set; } = true;

        /// <summary>
        /// 是否包含日志表
        /// </summary>
        public bool IncludeLogs { get; set; } = false;

        /// <summary>
        /// 备份说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否为手动触发
        /// </summary>
        public bool IsManual { get; set; } = true;
    }

    /// <summary>
    /// 备份结果
    /// </summary>
    public class BackupResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 备份记录
        /// </summary>
        public BackupRecord? Backup { get; set; }

        /// <summary>
        /// 错误消息（如果失败）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 备份耗时（秒）
        /// </summary>
        public double DurationSeconds { get; set; }
    }

    /// <summary>
    /// 备份配置
    /// </summary>
    public class BackupConfiguration
    {
        /// <summary>
        /// 是否启用自动备份
        /// </summary>
        public bool AutoBackupEnabled { get; set; } = true;

        /// <summary>
        /// 自动备份间隔（小时）
        /// </summary>
        public int AutoBackupIntervalHours { get; set; } = 24;

        /// <summary>
        /// 自动备份 Cron 表达式
        /// </summary>
        public string AutoBackupCron { get; set; } = "0 2 * * *";

        /// <summary>
        /// 备份保留天数
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// 最大备份数量
        /// </summary>
        public int MaxBackupCount { get; set; } = 50;

        /// <summary>
        /// 备份存储路径
        /// </summary>
        public string BackupPath { get; set; } = "backups";

        /// <summary>
        /// 是否压缩备份
        /// </summary>
        public bool CompressBackups { get; set; } = true;

        /// <summary>
        /// 自动备份包含日志
        /// </summary>
        public bool AutoBackupIncludeLogs { get; set; } = false;

        /// <summary>
        /// 上次自动备份时间
        /// </summary>
        public DateTime? LastAutoBackupAt { get; set; }
    }
}
