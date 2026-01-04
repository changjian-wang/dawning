namespace Dawning.Identity.Domain.Aggregates.Administration;

/// <summary>
/// Backup record domain model
/// </summary>
public class BackupRecord
{
    /// <summary>
    /// Backup ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Backup file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Backup file path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size (bytes)
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Backup type
    /// </summary>
    public string BackupType { get; set; } = string.Empty;

    /// <summary>
    /// Created time
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Backup description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether manually triggered
    /// </summary>
    public bool IsManual { get; set; }

    /// <summary>
    /// Backup status (success, failed, pending)
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// Error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// File size (formatted)
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
