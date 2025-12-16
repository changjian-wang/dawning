using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Application.Interfaces.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// 数据库备份服务实现
    /// </summary>
    public class BackupService : IBackupService
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<BackupService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _backupBasePath;

        public BackupService(
            IDbConnection connection,
            ILogger<BackupService> logger,
            IConfiguration configuration
        )
        {
            _connection = connection;
            _logger = logger;
            _configuration = configuration;
            _backupBasePath =
                configuration["Backup:Path"] ?? Path.Combine(AppContext.BaseDirectory, "backups");

            // 确保备份目录存在
            if (!Directory.Exists(_backupBasePath))
            {
                Directory.CreateDirectory(_backupBasePath);
            }
        }

        /// <inheritdoc />
        public async Task<BackupResult> CreateBackupAsync(BackupOptions? options = null)
        {
            options ??= new BackupOptions();
            var stopwatch = Stopwatch.StartNew();
            var backupId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"dawning_backup_{timestamp}_{backupId:N}.sql";
            var filePath = Path.Combine(_backupBasePath, fileName);

            var record = new BackupRecord
            {
                Id = backupId,
                FileName = fileName,
                FilePath = filePath,
                BackupType = options.BackupType,
                CreatedAt = DateTime.UtcNow,
                Description = options.Description,
                IsManual = options.IsManual,
                Status = "pending",
            };

            try
            {
                _logger.LogInformation("Starting database backup: {BackupId}", backupId);

                // 获取数据库连接信息
                var connectionString = _configuration.GetConnectionString("DawningIdentity");
                var (host, port, database, user, password) = ParseConnectionString(
                    connectionString ?? ""
                );

                // 生成 mysqldump 命令
                var dumpArgs = BuildMysqlDumpArgs(host, port, database, user, password, options);

                // 执行 mysqldump
                var success = await ExecuteMysqlDumpAsync(dumpArgs, filePath);

                if (!success)
                {
                    // 如果 mysqldump 失败，使用 SQL 导出作为备选
                    _logger.LogWarning("mysqldump failed, falling back to SQL export");
                    await ExportToSqlAsync(filePath, options);
                }

                // 压缩备份文件
                if (options.Compress && File.Exists(filePath))
                {
                    var compressedPath = filePath + ".gz";
                    await CompressFileAsync(filePath, compressedPath);
                    File.Delete(filePath);
                    filePath = compressedPath;
                    fileName += ".gz";
                    record.FileName = fileName;
                    record.FilePath = filePath;
                }

                // 获取文件大小
                var fileInfo = new FileInfo(filePath);
                record.FileSizeBytes = fileInfo.Length;
                record.Status = "success";

                stopwatch.Stop();

                // 保存备份记录到数据库
                await SaveBackupRecordAsync(record);

                _logger.LogInformation(
                    "Database backup completed: {BackupId}, Size: {Size}, Duration: {Duration}s",
                    backupId,
                    record.FileSizeFormatted,
                    stopwatch.Elapsed.TotalSeconds
                );

                return new BackupResult
                {
                    Success = true,
                    Backup = record,
                    DurationSeconds = stopwatch.Elapsed.TotalSeconds,
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database backup failed: {BackupId}", backupId);

                record.Status = "failed";
                record.ErrorMessage = ex.Message;
                await SaveBackupRecordAsync(record);

                // 清理失败的备份文件
                if (File.Exists(filePath))
                    File.Delete(filePath);
                if (File.Exists(filePath + ".gz"))
                    File.Delete(filePath + ".gz");

                return new BackupResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    DurationSeconds = stopwatch.Elapsed.TotalSeconds,
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<BackupRecord>> GetBackupHistoryAsync(int count = 20)
        {
            const string sql =
                @"
                SELECT 
                    id as Id, file_name as FileName, file_path as FilePath,
                    file_size_bytes as FileSizeBytes, backup_type as BackupType,
                    created_at as CreatedAt, description as Description,
                    is_manual as IsManual, status as Status, error_message as ErrorMessage
                FROM backup_records
                ORDER BY created_at DESC
                LIMIT @Count";

            var records = await _connection.QueryAsync<BackupRecord>(sql, new { Count = count });
            return records.ToList();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteBackupAsync(Guid backupId)
        {
            const string selectSql = "SELECT file_path FROM backup_records WHERE id = @Id";
            var filePath = await _connection.QueryFirstOrDefaultAsync<string>(
                selectSql,
                new { Id = backupId.ToString() }
            );

            if (filePath != null && File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            const string deleteSql = "DELETE FROM backup_records WHERE id = @Id";
            var affected = await _connection.ExecuteAsync(
                deleteSql,
                new { Id = backupId.ToString() }
            );
            return affected > 0;
        }

        /// <inheritdoc />
        public async Task<int> CleanupOldBackupsAsync(int retentionDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            const string selectSql =
                @"
                SELECT id as Id, file_path as FilePath 
                FROM backup_records 
                WHERE created_at < @CutoffDate";

            var oldBackups = await _connection.QueryAsync<dynamic>(
                selectSql,
                new { CutoffDate = cutoffDate }
            );
            var count = 0;

            foreach (var backup in oldBackups)
            {
                try
                {
                    if (backup.FilePath != null && File.Exists((string)backup.FilePath))
                    {
                        File.Delete((string)backup.FilePath);
                    }
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "Failed to delete backup file: {FilePath} - {Error}",
                        (string?)backup.FilePath,
                        ex.Message
                    );
                }
            }

            const string deleteSql = "DELETE FROM backup_records WHERE created_at < @CutoffDate";
            await _connection.ExecuteAsync(deleteSql, new { CutoffDate = cutoffDate });

            _logger.LogInformation("Cleaned up {Count} old backups", count);
            return count;
        }

        /// <inheritdoc />
        public async Task<BackupConfiguration> GetConfigurationAsync()
        {
            var config = new BackupConfiguration();

            const string sql =
                @"
                SELECT config_key, config_value 
                FROM system_configs 
                WHERE config_group = 'Backup'";

            var configs = await _connection.QueryAsync<dynamic>(sql);
            foreach (var c in configs)
            {
                string key = c.config_key;
                string? value = c.config_value;

                switch (key)
                {
                    case "AutoBackupEnabled":
                        config.AutoBackupEnabled = bool.TryParse(value, out var enabled) && enabled;
                        break;
                    case "AutoBackupIntervalHours":
                        config.AutoBackupIntervalHours = int.TryParse(value, out var hours)
                            ? hours
                            : 24;
                        break;
                    case "RetentionDays":
                        config.RetentionDays = int.TryParse(value, out var days) ? days : 30;
                        break;
                    case "MaxBackupCount":
                        config.MaxBackupCount = int.TryParse(value, out var maxCount)
                            ? maxCount
                            : 50;
                        break;
                    case "BackupPath":
                        config.BackupPath = value ?? "backups";
                        break;
                    case "CompressBackups":
                        config.CompressBackups = bool.TryParse(value, out var compress) && compress;
                        break;
                    case "AutoBackupIncludeLogs":
                        config.AutoBackupIncludeLogs =
                            bool.TryParse(value, out var includeLogs) && includeLogs;
                        break;
                    case "LastAutoBackupAt":
                        if (DateTime.TryParse(value, out var lastBackup))
                            config.LastAutoBackupAt = lastBackup;
                        break;
                }
            }

            return config;
        }

        /// <inheritdoc />
        public async Task UpdateConfigurationAsync(BackupConfiguration config)
        {
            var updates = new Dictionary<string, string>
            {
                ["AutoBackupEnabled"] = config.AutoBackupEnabled.ToString().ToLower(),
                ["AutoBackupIntervalHours"] = config.AutoBackupIntervalHours.ToString(),
                ["RetentionDays"] = config.RetentionDays.ToString(),
                ["MaxBackupCount"] = config.MaxBackupCount.ToString(),
                ["BackupPath"] = config.BackupPath,
                ["CompressBackups"] = config.CompressBackups.ToString().ToLower(),
                ["AutoBackupIncludeLogs"] = config.AutoBackupIncludeLogs.ToString().ToLower(),
            };

            foreach (var (key, value) in updates)
            {
                const string sql =
                    @"
                    INSERT INTO system_configs (id, config_group, config_key, config_value, created_at, updated_at)
                    VALUES (UUID(), 'Backup', @Key, @Value, NOW(), NOW())
                    ON DUPLICATE KEY UPDATE config_value = @Value, updated_at = NOW()";

                await _connection.ExecuteAsync(sql, new { Key = key, Value = value });
            }
        }

        #region Private Methods

        private static (
            string host,
            int port,
            string database,
            string user,
            string password
        ) ParseConnectionString(string connectionString)
        {
            var parts = connectionString
                .Split(';')
                .Select(p => p.Split('='))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());

            var host =
                parts.GetValueOrDefault("server") ?? parts.GetValueOrDefault("host") ?? "localhost";
            var portStr = parts.GetValueOrDefault("port") ?? "3306";
            var database =
                parts.GetValueOrDefault("database")
                ?? parts.GetValueOrDefault("initial catalog")
                ?? "";
            var user =
                parts.GetValueOrDefault("user id")
                ?? parts.GetValueOrDefault("uid")
                ?? parts.GetValueOrDefault("user")
                ?? "root";
            var password =
                parts.GetValueOrDefault("password") ?? parts.GetValueOrDefault("pwd") ?? "";

            return (
                host,
                int.TryParse(portStr, out var port) ? port : 3306,
                database,
                user,
                password
            );
        }

        private static string BuildMysqlDumpArgs(
            string host,
            int port,
            string database,
            string user,
            string password,
            BackupOptions options
        )
        {
            var args = $"-h {host} -P {port} -u {user}";
            if (!string.IsNullOrEmpty(password))
            {
                args += $" -p{password}";
            }

            args += " --single-transaction --routines --triggers";

            // 排除日志表（如果不需要）
            if (!options.IncludeLogs)
            {
                args += $" --ignore-table={database}.request_logs";
                args += $" --ignore-table={database}.audit_logs";
            }

            args += $" {database}";
            return args;
        }

        private async Task<bool> ExecuteMysqlDumpAsync(string args, string outputPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "mysqldump",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = startInfo };
                using var output = new FileStream(
                    outputPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                );

                process.Start();

                var copyTask = process.StandardOutput.BaseStream.CopyToAsync(output);
                var errorTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(copyTask, errorTask);
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await errorTask;
                    _logger.LogWarning(
                        "mysqldump exited with code {ExitCode}: {Error}",
                        process.ExitCode,
                        error
                    );
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to execute mysqldump");
                return false;
            }
        }

        private async Task ExportToSqlAsync(string outputPath, BackupOptions options)
        {
            await using var writer = new StreamWriter(outputPath);

            // 写入头部信息
            await writer.WriteLineAsync("-- Dawning Gateway Database Backup");
            await writer.WriteLineAsync(
                $"-- Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
            );
            await writer.WriteLineAsync($"-- Backup Type: {options.BackupType}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("SET FOREIGN_KEY_CHECKS=0;");
            await writer.WriteLineAsync();

            // 获取所有表
            var tables = await _connection.QueryAsync<string>("SHOW TABLES");

            foreach (var table in tables)
            {
                // 跳过日志表（如果不需要）
                if (!options.IncludeLogs && (table == "request_logs" || table == "audit_logs"))
                {
                    continue;
                }

                // 写入表结构
                var createTable = await _connection.QueryFirstAsync<dynamic>(
                    $"SHOW CREATE TABLE `{table}`"
                );
                await writer.WriteLineAsync($"-- Table: {table}");
                await writer.WriteLineAsync($"DROP TABLE IF EXISTS `{table}`;");
                await writer.WriteLineAsync(
                    ((IDictionary<string, object>)createTable).Values.Last()?.ToString() + ";"
                );
                await writer.WriteLineAsync();

                // 写入数据
                var data = await _connection.QueryAsync($"SELECT * FROM `{table}`");
                var dataList = data.ToList();

                if (dataList.Count > 0)
                {
                    foreach (var row in dataList)
                    {
                        var dict = (IDictionary<string, object>)row;
                        var columns = string.Join(", ", dict.Keys.Select(k => $"`{k}`"));
                        var values = string.Join(", ", dict.Values.Select(FormatValue));
                        await writer.WriteLineAsync(
                            $"INSERT INTO `{table}` ({columns}) VALUES ({values});"
                        );
                    }
                    await writer.WriteLineAsync();
                }
            }

            await writer.WriteLineAsync("SET FOREIGN_KEY_CHECKS=1;");
        }

        private static string FormatValue(object? value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";
            if (value is bool b)
                return b ? "1" : "0";
            if (value is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            if (value is byte[] bytes)
                return $"X'{BitConverter.ToString(bytes).Replace("-", "")}'";
            if (value is int or long or float or double or decimal)
                return value.ToString() ?? "NULL";

            var str = value.ToString() ?? "";
            return $"'{str.Replace("'", "''").Replace("\\", "\\\\")}'";
        }

        private static async Task CompressFileAsync(string sourcePath, string destPath)
        {
            await using var sourceStream = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read
            );
            await using var destStream = new FileStream(
                destPath,
                FileMode.Create,
                FileAccess.Write
            );
            await using var gzipStream = new GZipStream(destStream, CompressionLevel.Optimal);
            await sourceStream.CopyToAsync(gzipStream);
        }

        private async Task SaveBackupRecordAsync(BackupRecord record)
        {
            const string sql =
                @"
                INSERT INTO backup_records 
                (id, file_name, file_path, file_size_bytes, backup_type, created_at, description, is_manual, status, error_message)
                VALUES 
                (@Id, @FileName, @FilePath, @FileSizeBytes, @BackupType, @CreatedAt, @Description, @IsManual, @Status, @ErrorMessage)";

            await _connection.ExecuteAsync(
                sql,
                new
                {
                    Id = record.Id.ToString(),
                    record.FileName,
                    record.FilePath,
                    record.FileSizeBytes,
                    record.BackupType,
                    record.CreatedAt,
                    record.Description,
                    record.IsManual,
                    record.Status,
                    record.ErrorMessage,
                }
            );
        }

        #endregion
    }
}
