using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;

namespace Dawning.Identity.Application.Services.Administration;

/// <summary>
/// 数据库备份服务实现 - 使用Repository模式
/// </summary>
public class BackupService : IBackupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemConfigService _configService;
    private readonly IDatabaseExportService _exportService;
    private readonly string _backupPath;

    public BackupService(
        IUnitOfWork unitOfWork,
        ISystemConfigService configService,
        IDatabaseExportService exportService
    )
    {
        _unitOfWork = unitOfWork;
        _configService = configService;
        _exportService = exportService;
        _backupPath = Path.Combine(AppContext.BaseDirectory, "Backups");

        // 确保备份目录存在
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }

    /// <summary>
    /// 获取备份历史记录
    /// </summary>
    public async Task<List<BackupRecord>> GetBackupHistoryAsync(int count = 20)
    {
        return await _unitOfWork.BackupRecord.GetHistoryAsync(count);
    }

    /// <summary>
    /// 创建数据库备份
    /// </summary>
    public async Task<BackupResult> CreateBackupAsync(BackupOptions? options = null)
    {
        var startTime = DateTime.UtcNow;
        options ??= new BackupOptions();

        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"backup_{timestamp}.sql";
            var filePath = Path.Combine(_backupPath, fileName);

            // 构建排除表集合
            var excludeTables = BuildExcludeTableSet(options);

            // 使用导出服务导出数据库
            var sqlContent = await _exportService.ExportToSqlAsync(
                _unitOfWork.Connection,
                excludeTables
            );
            await File.WriteAllTextAsync(filePath, sqlContent);

            var fileInfo = new FileInfo(filePath);

            var record = new BackupRecord
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                FilePath = filePath,
                FileSizeBytes = fileInfo.Length,
                Description = options.Description,
                BackupType = options.BackupType,
                IsManual = options.IsManual,
                Status = "success",
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.BackupRecord.CreateAsync(record);

            return new BackupResult
            {
                Success = true,
                Backup = record,
                DurationSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
            };
        }
        catch (Exception ex)
        {
            return new BackupResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                DurationSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
            };
        }
    }

    /// <summary>
    /// 删除备份记录
    /// </summary>
    public async Task<bool> DeleteBackupAsync(Guid backupId)
    {
        var record = await _unitOfWork.BackupRecord.GetByIdAsync(backupId);
        if (record == null)
        {
            return false;
        }

        // 删除物理文件
        if (File.Exists(record.FilePath))
        {
            File.Delete(record.FilePath);
        }

        // 删除数据库记录
        await _unitOfWork.BackupRecord.DeleteAsync(backupId);
        return true;
    }

    /// <summary>
    /// 清理过期备份
    /// </summary>
    public async Task<int> CleanupOldBackupsAsync(int retentionDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var expiredRecords = await _unitOfWork.BackupRecord.GetExpiredAsync(cutoffDate);

        foreach (var record in expiredRecords)
        {
            // 删除物理文件
            if (File.Exists(record.FilePath))
            {
                try
                {
                    File.Delete(record.FilePath);
                }
                catch
                {
                    // 忽略文件删除错误，继续删除数据库记录
                }
            }
        }

        // 批量删除过期记录
        return await _unitOfWork.BackupRecord.DeleteExpiredAsync(cutoffDate);
    }

    /// <summary>
    /// 获取备份配置
    /// </summary>
    public async Task<BackupConfiguration> GetConfigurationAsync()
    {
        var retentionDays = await _configService.GetValueAsync("backup", "retention_days");
        var autoBackupEnabled = await _configService.GetValueAsync("backup", "auto_enabled");
        var autoBackupCron = await _configService.GetValueAsync("backup", "auto_cron");

        return new BackupConfiguration
        {
            RetentionDays = int.TryParse(retentionDays, out var days) ? days : 30,
            AutoBackupEnabled = autoBackupEnabled?.ToLower() == "true",
            AutoBackupCron = autoBackupCron ?? "0 2 * * *", // 默认每天凌晨2点
        };
    }

    /// <summary>
    /// 更新备份配置
    /// </summary>
    public async Task UpdateConfigurationAsync(BackupConfiguration config)
    {
        await _configService.SetValueAsync(
            "backup",
            "retention_days",
            config.RetentionDays.ToString(),
            "Backup retention days"
        );
        await _configService.SetValueAsync(
            "backup",
            "auto_enabled",
            config.AutoBackupEnabled.ToString().ToLower(),
            "Auto backup enabled"
        );
        await _configService.SetValueAsync(
            "backup",
            "auto_cron",
            config.AutoBackupCron,
            "Auto backup cron expression"
        );
    }

    /// <summary>
    /// 构建排除表集合
    /// </summary>
    private static HashSet<string> BuildExcludeTableSet(BackupOptions options)
    {
        var excludeTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!options.IncludeLogs)
        {
            excludeTables.Add("audit_logs");
            excludeTables.Add("system_logs");
        }

        if (!options.IncludeConfigs)
        {
            excludeTables.Add("system_configs");
        }

        return excludeTables;
    }
}
