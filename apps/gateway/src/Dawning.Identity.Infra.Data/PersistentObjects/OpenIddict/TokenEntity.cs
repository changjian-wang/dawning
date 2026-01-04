using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// OpenIddict token persistent object
    /// </summary>
    [Table("openiddict_tokens")]
    public class TokenEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the application associated with this entity.
        /// </summary>
        [Column("application_id")]
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the authorization associated with this entity.
        /// </summary>
        [Column("authorization_id")]
        public Guid? AuthorizationId { get; set; }

        /// <summary>
        /// Gets or sets the subject associated with the entity.
        /// </summary>
        [Column("subject")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the type associated with the entity.
        /// </summary>
        [Column("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the entity.
        /// </summary>
        [Column("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the payload data associated with the entity.
        /// </summary>
        [Column("payload")]
        public string? Payload { get; set; }

        /// <summary>
        /// Gets or sets the reference identifier associated with the entity.
        /// </summary>
        [Column("reference_id")]
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time for the entity.
        /// </summary>
        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp representing the number of milliseconds since the Unix epoch (January 1, 1970,
        /// 00:00:00 UTC).
        /// </summary>
        [Column("timestamp")]
        [IgnoreUpdate]
        [DefaultSortName]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Gets or sets the timestamp indicating when the entity was created.
        /// </summary>
        [Column("created_at")]
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
