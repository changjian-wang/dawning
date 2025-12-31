using System;

namespace Dawning.Identity.Application.Dtos.Administration
{
    /// <summary>
    /// Audit log DTO
    /// </summary>
    public class AuditLogDto
    {
        /// <summary>
        /// Audit log ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Operator user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operator username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Entity type
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Entity ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Operation description
        /// </summary>
        public string? Description { get; set; }

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
        /// Request method
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Values before modification
        /// </summary>
        public object? OldValues { get; set; }

        /// <summary>
        /// Values after modification
        /// </summary>
        public object? NewValues { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Create audit log DTO
    /// </summary>
    public class CreateAuditLogDto
    {
        /// <summary>
        /// Operator user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operator username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Entity type
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Entity ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Operation description
        /// </summary>
        public string? Description { get; set; }

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
        /// Request method
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Values before modification
        /// </summary>
        public object? OldValues { get; set; }

        /// <summary>
        /// Values after modification
        /// </summary>
        public object? NewValues { get; set; }
    }
}
