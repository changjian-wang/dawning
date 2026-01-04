using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// System log aggregate root
    /// Used for recording application logs and exception information
    /// </summary>
    public class SystemLog : IAggregateRoot
    {
        /// <summary>
        /// Log unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Log level (Info, Warning, Error)
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Log message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception information (exception type and message)
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Exception stack trace
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Exception source
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Operating user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operating username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request path
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// Request method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp (used for sorting and query optimization)
        /// </summary>
        public long Timestamp { get; set; }
    }
}
