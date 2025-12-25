using System;

namespace Dawning.Identity.Domain.Models.Monitoring;

/// <summary>
/// 请求日志查询模型
/// </summary>
public class RequestLogQueryModel
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int? StatusCode { get; set; }
    public int? MinStatusCode { get; set; }
    public int? MaxStatusCode { get; set; }
    public Guid? UserId { get; set; }
    public string? ClientIp { get; set; }
    public bool? OnlyErrors { get; set; }
    public long? SlowRequestThresholdMs { get; set; }
}
