namespace Dawning.Identity.Domain.Aggregates.Administration;

/// <summary>
/// 备份记录领域模型
/// </summary>
public class BackupRecord
{
    /// <summary>
    /// 备份ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 备份文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 备份文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// 备份类型
    /// </summary>
    public string BackupType { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 备份说明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否为手动触发
    /// </summary>
    public bool IsManual { get; set; }

    /// <summary>
    /// 备份状态（success, failed, pending）
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 文件大小（格式化）
    /// </summary>
    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

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
