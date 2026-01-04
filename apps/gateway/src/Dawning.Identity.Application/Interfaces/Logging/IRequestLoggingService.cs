using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Logging
{
    /// <summary>
    /// Request logging service interface
    /// </summary>
    public interface IRequestLoggingService
    {
        /// <summary>
        /// Log a request
        /// </summary>
        Task LogRequestAsync(RequestLogEntry entry);

        /// <summary>
        /// Get request logs (paginated)
        /// </summary>
        Task<PagedRequestLogs> GetLogsAsync(RequestLogQuery query);

        /// <summary>
        /// Get request statistics
        /// </summary>
        Task<RequestStatistics> GetStatisticsAsync(
            DateTime? startTime = null,
            DateTime? endTime = null
        );

        /// <summary>
        /// Clean up expired logs
        /// </summary>
        Task<int> CleanupOldLogsAsync(int retentionDays);
    }

    /// <summary>
    /// Request log entry
    /// </summary>
    public class RequestLogEntry
    {
        /// <summary>
        /// Log ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Request ID (for correlation)
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// HTTP method
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Request path
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Query string
        /// </summary>
        public string? QueryString { get; set; }

        /// <summary>
        /// Status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response time (milliseconds)
        /// </summary>
        public long ResponseTimeMs { get; set; }

        /// <summary>
        /// Client IP
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// User ID (if authenticated)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Request time
        /// </summary>
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request body size (bytes)
        /// </summary>
        public long? RequestBodySize { get; set; }

        /// <summary>
        /// Response body size (bytes)
        /// </summary>
        public long? ResponseBodySize { get; set; }

        /// <summary>
        /// Exception information (if any)
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Additional information (JSON)
        /// </summary>
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Request log query parameters
    /// </summary>
    public class RequestLogQuery
    {
        /// <summary>
        /// Start time
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// HTTP method filter
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Path filter (supports fuzzy matching)
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Status code filter
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Minimum status code
        /// </summary>
        public int? MinStatusCode { get; set; }

        /// <summary>
        /// Maximum status code
        /// </summary>
        public int? MaxStatusCode { get; set; }

        /// <summary>
        /// User ID filter
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Client IP filter
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// Show only error requests
        /// </summary>
        public bool? OnlyErrors { get; set; }

        /// <summary>
        /// Show only slow requests (greater than specified milliseconds)
        /// </summary>
        public long? SlowRequestThresholdMs { get; set; }

        /// <summary>
        /// Page number
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Paginated request logs result
    /// </summary>
    public class PagedRequestLogs
    {
        /// <summary>
        /// Log list
        /// </summary>
        public List<RequestLogEntry> Items { get; set; } = new();

        /// <summary>
        /// Total count
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// Request statistics
    /// </summary>
    public class RequestStatistics
    {
        /// <summary>
        /// Total requests
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Successful requests (2xx)
        /// </summary>
        public long SuccessRequests { get; set; }

        /// <summary>
        /// Client errors (4xx)
        /// </summary>
        public long ClientErrors { get; set; }

        /// <summary>
        /// Server errors (5xx)
        /// </summary>
        public long ServerErrors { get; set; }

        /// <summary>
        /// Average response time (milliseconds)
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// Maximum response time (milliseconds)
        /// </summary>
        public long MaxResponseTimeMs { get; set; }

        /// <summary>
        /// Minimum response time (milliseconds)
        /// </summary>
        public long MinResponseTimeMs { get; set; }

        /// <summary>
        /// P95 response time (milliseconds)
        /// </summary>
        public long P95ResponseTimeMs { get; set; }

        /// <summary>
        /// P99 response time (milliseconds)
        /// </summary>
        public long P99ResponseTimeMs { get; set; }

        /// <summary>
        /// Statistics grouped by status code
        /// </summary>
        public Dictionary<int, long> StatusCodeDistribution { get; set; } = new();

        /// <summary>
        /// Statistics grouped by path (Top 10)
        /// </summary>
        public List<PathStatistic> TopPaths { get; set; } = new();

        /// <summary>
        /// Requests grouped by hour
        /// </summary>
        public Dictionary<string, long> HourlyRequests { get; set; } = new();

        /// <summary>
        /// Statistics time range start
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Statistics time range end
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// Path statistics
    /// </summary>
    public class PathStatistic
    {
        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Request count
        /// </summary>
        public long RequestCount { get; set; }

        /// <summary>
        /// Average response time
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// Error count
        /// </summary>
        public long ErrorCount { get; set; }
    }
}
