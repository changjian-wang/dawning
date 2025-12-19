using System;

namespace Dawning.Identity.Domain.Aggregates.Monitoring;

/// <summary>
/// 请求日志聚合根
/// </summary>
public class RequestLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RequestId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime RequestTime { get; set; }
    public long? RequestBodySize { get; set; }
    public long? ResponseBodySize { get; set; }
    public string? Exception { get; set; }
    public string? AdditionalInfo { get; set; }
}
