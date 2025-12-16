using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers
{
    /// <summary>
    /// 数据库备份管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        /// <summary>
        /// 创建数据库备份
        /// </summary>
        /// <param name="options">备份选项</param>
        /// <returns>备份结果</returns>
        [HttpPost]
        public async Task<ActionResult<BackupResult>> CreateBackup([FromBody] BackupOptions? options = null)
        {
            var result = await _backupService.CreateBackupAsync(options ?? new BackupOptions { IsManual = true });
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// 获取备份历史列表
        /// </summary>
        /// <param name="count">返回数量（默认20）</param>
        /// <returns>备份记录列表</returns>
        [HttpGet("history")]
        public async Task<ActionResult> GetBackupHistory([FromQuery] int count = 20)
        {
            var history = await _backupService.GetBackupHistoryAsync(Math.Min(count, 100));
            return Ok(history);
        }

        /// <summary>
        /// 删除指定备份
        /// </summary>
        /// <param name="id">备份ID</param>
        /// <returns>是否成功</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBackup(Guid id)
        {
            var success = await _backupService.DeleteBackupAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = "备份不存在" });
            }
            
            return Ok(new { message = "备份已删除" });
        }

        /// <summary>
        /// 清理过期备份
        /// </summary>
        /// <param name="retentionDays">保留天数（默认30）</param>
        /// <returns>清理结果</returns>
        [HttpPost("cleanup")]
        public async Task<ActionResult> CleanupOldBackups([FromQuery] int retentionDays = 30)
        {
            if (retentionDays < 1)
            {
                return BadRequest(new { message = "保留天数必须大于0" });
            }
            
            var count = await _backupService.CleanupOldBackupsAsync(retentionDays);
            
            return Ok(new 
            { 
                message = $"已清理 {count} 个过期备份",
                deletedCount = count,
                retentionDays = retentionDays
            });
        }

        /// <summary>
        /// 获取备份配置
        /// </summary>
        /// <returns>备份配置</returns>
        [HttpGet("config")]
        public async Task<ActionResult<BackupConfiguration>> GetConfiguration()
        {
            var config = await _backupService.GetConfigurationAsync();
            return Ok(config);
        }

        /// <summary>
        /// 更新备份配置
        /// </summary>
        /// <param name="config">新配置</param>
        /// <returns>更新结果</returns>
        [HttpPut("config")]
        public async Task<ActionResult> UpdateConfiguration([FromBody] BackupConfiguration config)
        {
            await _backupService.UpdateConfigurationAsync(config);
            return Ok(new { message = "备份配置已更新" });
        }

        /// <summary>
        /// 获取备份存储信息
        /// </summary>
        /// <returns>存储信息</returns>
        [HttpGet("storage-info")]
        public async Task<ActionResult> GetStorageInfo()
        {
            var history = await _backupService.GetBackupHistoryAsync(1000);
            var totalSize = 0L;
            var successCount = 0;
            var failedCount = 0;

            foreach (var record in history)
            {
                totalSize += record.FileSizeBytes;
                if (record.Status == "success")
                    successCount++;
                else if (record.Status == "failed")
                    failedCount++;
            }

            return Ok(new
            {
                totalBackups = history.Count,
                successfulBackups = successCount,
                failedBackups = failedCount,
                totalSizeBytes = totalSize,
                totalSizeFormatted = FormatFileSize(totalSize),
                oldestBackup = history.Count > 0 ? history[^1].CreatedAt : (DateTime?)null,
                latestBackup = history.Count > 0 ? history[0].CreatedAt : (DateTime?)null
            });
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
