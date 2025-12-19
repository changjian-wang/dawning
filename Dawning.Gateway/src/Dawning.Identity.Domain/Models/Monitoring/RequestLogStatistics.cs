using System;
using System.Collections.Generic;

namespace Dawning.Identity.Domain.Models.Monitoring;

/// <summary>
/// 请求日志统计模型
/// </summary>
public class RequestLogStatistics
{
    public long TotalRequests { get; set; }
    public long SuccessRequests { get; set; }
    public long ClientErrors { get; set; }
    public long ServerErrors { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long MaxResponseTimeMs { get; set; }
    public long MinResponseTimeMs { get; set; }
    public long P95ResponseTimeMs { get; set; }
    public long P99ResponseTimeMs { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Dictionary<int, long> StatusCodeDistribution { get; set; } = new();
    public List<PathStatisticModel> TopPaths { get; set; } = new();
    public Dictionary<string, long> HourlyRequests { get; set; } = new();
}

/// <summary>
/// 路径统计模型
/// </summary>
public class PathStatisticModel
{
    public string Path { get; set; } = string.Empty;
    public long RequestCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long ErrorCount { get; set; }
}
