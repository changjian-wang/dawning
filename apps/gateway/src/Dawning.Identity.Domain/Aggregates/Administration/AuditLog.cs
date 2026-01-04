using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// Audit log aggregate root
    /// </summary>
    public class AuditLog : IAggregateRoot
    {
        /// <summary>
        /// Audit log unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Operating user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operating username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Action type (Create, Update, Delete, Login, Logout, etc.)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Entity type (User, Role, Application, etc.)
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
        /// Value before modification (JSON string)
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// Value after modification (JSON string)
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; }
    }
}
