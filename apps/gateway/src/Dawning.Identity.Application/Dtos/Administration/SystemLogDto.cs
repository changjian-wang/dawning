using System;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// System Log DTO
    /// </summary>
    public class SystemLogDto
    {
        /// <summary>
        /// Log ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Log Level
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Log Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception Information
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Exception Stack Trace
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Exception Source
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP Address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request Path
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// Request Method
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Created Time
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Create System Log DTO
    /// </summary>
    public class CreateSystemLogDto
    {
        /// <summary>
        /// Log Level
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Log Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception Information
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Exception Stack Trace
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Exception Source
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP Address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request Path
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// Request Method
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int? StatusCode { get; set; }
    }
}
