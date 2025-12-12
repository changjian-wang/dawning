using Dawning.Identity.Domain.Core.Interfaces;
using System;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 系统日志聚合根
    /// 用于记录应用程序日志和异常信息
    /// </summary>
    public class SystemLog : IAggregateRoot
    {
        /// <summary>
        /// 日志唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 日志级别 (Info, Warning, Error)
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息（异常类型和消息）
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 异常堆栈跟踪
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// 异常来源
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// 操作用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// 请求方法 (GET, POST, PUT, DELETE等)
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 时间戳（用于排序和查询优化）
        /// </summary>
        public long Timestamp { get; set; }
    }
}
