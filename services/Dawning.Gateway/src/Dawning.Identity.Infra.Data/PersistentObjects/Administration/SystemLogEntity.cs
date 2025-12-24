using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 系统日志数据库实体
    /// </summary>
    [Table("system_logs")]
    public class SystemLogEntity
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        [Column("level")]
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// 日志消息
        /// </summary>
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息
        /// </summary>
        [Column("exception")]
        public string? Exception { get; set; }

        /// <summary>
        /// 异常堆栈跟踪
        /// </summary>
        [Column("stack_trace")]
        public string? StackTrace { get; set; }

        /// <summary>
        /// 异常来源
        /// </summary>
        [Column("source")]
        public string? Source { get; set; }

        /// <summary>
        /// 操作用户ID
        /// </summary>
        [Column("user_id")]
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作用户名
        /// </summary>
        [Column("username")]
        public string? Username { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        [Column("request_path")]
        public string? RequestPath { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        [Column("request_method")]
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        [Column("status_code")]
        public int? StatusCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [Column("timestamp")]
        public long Timestamp { get; set; }
    }
}
