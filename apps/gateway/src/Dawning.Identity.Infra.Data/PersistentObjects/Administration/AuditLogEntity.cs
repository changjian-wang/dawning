using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// Audit log database entity
    /// </summary>
    [Table("audit_logs")]
    public class AuditLogEntity
    {
        /// <summary>
        /// Audit log ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

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
        /// Action type
        /// </summary>
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Entity type
        /// </summary>
        [Column("entity_type")]
        public string? EntityType { get; set; }

        /// <summary>
        /// Entity ID
        /// </summary>
        [Column("entity_id")]
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Action description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

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
        /// Old values before modification (JSON string)
        /// </summary>
        [Column("old_values")]
        public string? OldValues { get; set; }

        /// <summary>
        /// New values after modification (JSON string)
        /// </summary>
        [Column("new_values")]
        public string? NewValues { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp (computed field, not mapped to database)
        /// </summary>
        [Computed]
        public long Timestamp { get; set; }
    }
}
