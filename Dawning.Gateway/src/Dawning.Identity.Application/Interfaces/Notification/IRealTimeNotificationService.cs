namespace Dawning.Identity.Application.Interfaces.Notification;

/// <summary>
/// 实时推送服务接口
/// 用于将告警、系统消息等实时推送给用户
/// </summary>
public interface IRealTimeNotificationService
{
    /// <summary>
    /// 推送告警通知
    /// </summary>
    Task SendAlertNotificationAsync(RealTimeAlertNotification alert);

    /// <summary>
    /// 推送系统消息
    /// </summary>
    Task SendSystemMessageAsync(RealTimeSystemMessage message);

    /// <summary>
    /// 推送日志条目（实时日志流）
    /// </summary>
    Task SendLogEntryAsync(RealTimeLogEntry logEntry);

    /// <summary>
    /// 推送通知给指定用户
    /// </summary>
    Task SendToUserAsync(Guid userId, RealTimeNotification notification);

    /// <summary>
    /// 推送通知给指定角色
    /// </summary>
    Task SendToRoleAsync(string role, RealTimeNotification notification);
}

/// <summary>
/// 实时通知基类
/// </summary>
public class RealTimeNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// 实时告警通知
/// </summary>
public class RealTimeAlertNotification : RealTimeNotification
{
    public string Severity { get; set; } = "warning";
    public long? RuleId { get; set; }
    public string? RuleName { get; set; }
    public string? MetricType { get; set; }
    public decimal? Value { get; set; }
    public decimal? Threshold { get; set; }
}

/// <summary>
/// 实时系统消息
/// </summary>
public class RealTimeSystemMessage : RealTimeNotification
{
    public bool RequireAcknowledge { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// 实时日志条目
/// </summary>
public class RealTimeLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
}
