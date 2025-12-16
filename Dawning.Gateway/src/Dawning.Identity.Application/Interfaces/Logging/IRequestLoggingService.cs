using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Logging
{
    /// <summary>
    /// 请求日志服务接口
    /// </summary>
    public interface IRequestLoggingService
    {
        /// <summary>
        /// 记录请求日志
        /// </summary>
        Task LogRequestAsync(RequestLogEntry entry);

        /// <summary>
        /// 获取请求日志（分页）
        /// </summary>
        Task<PagedRequestLogs> GetLogsAsync(RequestLogQuery query);

        /// <summary>
        /// 获取请求统计信息
        /// </summary>
        Task<RequestStatistics> GetStatisticsAsync(
            DateTime? startTime = null,
            DateTime? endTime = null
        );

        /// <summary>
        /// 清理过期日志
        /// </summary>
        Task<int> CleanupOldLogsAsync(int retentionDays);
    }

    /// <summary>
    /// 请求日志条目
    /// </summary>
    public class RequestLogEntry
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 请求ID（用于关联）
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// HTTP 方法
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// 请求路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 查询字符串
        /// </summary>
        public string? QueryString { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long ResponseTimeMs { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 用户ID（如果已认证）
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 请求体大小（字节）
        /// </summary>
        public long? RequestBodySize { get; set; }

        /// <summary>
        /// 响应体大小（字节）
        /// </summary>
        public long? ResponseBodySize { get; set; }

        /// <summary>
        /// 异常信息（如果有）
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 额外信息（JSON）
        /// </summary>
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// 请求日志查询参数
    /// </summary>
    public class RequestLogQuery
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// HTTP 方法过滤
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// 路径过滤（支持模糊匹配）
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 状态码过滤
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// 最小状态码
        /// </summary>
        public int? MinStatusCode { get; set; }

        /// <summary>
        /// 最大状态码
        /// </summary>
        public int? MaxStatusCode { get; set; }

        /// <summary>
        /// 用户ID过滤
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 客户端IP过滤
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// 只显示错误请求
        /// </summary>
        public bool? OnlyErrors { get; set; }

        /// <summary>
        /// 只显示慢请求（大于指定毫秒数）
        /// </summary>
        public long? SlowRequestThresholdMs { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 分页请求日志结果
    /// </summary>
    public class PagedRequestLogs
    {
        /// <summary>
        /// 日志列表
        /// </summary>
        public List<RequestLogEntry> Items { get; set; } = new();

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 请求统计信息
    /// </summary>
    public class RequestStatistics
    {
        /// <summary>
        /// 总请求数
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// 成功请求数（2xx）
        /// </summary>
        public long SuccessRequests { get; set; }

        /// <summary>
        /// 客户端错误数（4xx）
        /// </summary>
        public long ClientErrors { get; set; }

        /// <summary>
        /// 服务器错误数（5xx）
        /// </summary>
        public long ServerErrors { get; set; }

        /// <summary>
        /// 平均响应时间（毫秒）
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// 最大响应时间（毫秒）
        /// </summary>
        public long MaxResponseTimeMs { get; set; }

        /// <summary>
        /// 最小响应时间（毫秒）
        /// </summary>
        public long MinResponseTimeMs { get; set; }

        /// <summary>
        /// P95 响应时间（毫秒）
        /// </summary>
        public long P95ResponseTimeMs { get; set; }

        /// <summary>
        /// P99 响应时间（毫秒）
        /// </summary>
        public long P99ResponseTimeMs { get; set; }

        /// <summary>
        /// 按状态码分组统计
        /// </summary>
        public Dictionary<int, long> StatusCodeDistribution { get; set; } = new();

        /// <summary>
        /// 按路径分组统计（Top 10）
        /// </summary>
        public List<PathStatistic> TopPaths { get; set; } = new();

        /// <summary>
        /// 按小时分组请求量
        /// </summary>
        public Dictionary<string, long> HourlyRequests { get; set; } = new();

        /// <summary>
        /// 统计时间范围
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 统计时间范围
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// 路径统计
    /// </summary>
    public class PathStatistic
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 请求次数
        /// </summary>
        public long RequestCount { get; set; }

        /// <summary>
        /// 平均响应时间
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        public long ErrorCount { get; set; }
    }
}
