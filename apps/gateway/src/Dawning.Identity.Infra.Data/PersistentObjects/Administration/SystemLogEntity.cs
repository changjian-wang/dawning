using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// System log database entity
    /// </summary>
    [Table("system_logs")]
    public class SystemLogEntity
    {
        /// <summary>
        /// Log ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Log level
        /// </summary>
        [Column("level")]
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Log message
        /// </summary>
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception information
        /// </summary>
        [Column("exception")]
        public string? Exception { get; set; }

        /// <summary>
        /// Exception stack trace
        /// </summary>
        [Column("stack_trace")]
        public string? StackTrace { get; set; }

        /// <summary>
        /// Exception source
        /// </summary>
        [Column("source")]
        public string? Source { get; set; }

        /// <summary>
        /// Operator user ID
        /// </summary>
        [Column("user_id")]
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operator username
        /// </summary>
        [Column("username")]
        public string? Username { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request path
        /// </summary>
        [Column("request_path")]
        public string? RequestPath { get; set; }

        /// <summary>
        /// Request method
        /// </summary>
        [Column("request_method")]
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        [Column("status_code")]
        public int? StatusCode { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [Column("timestamp")]
        public long Timestamp { get; set; }
    }
}
