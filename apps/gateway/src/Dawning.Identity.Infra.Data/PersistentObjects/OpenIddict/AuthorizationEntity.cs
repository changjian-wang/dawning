using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// OpenIddict Authorization persistent object
    /// </summary>
    [Table("openiddict_authorizations")]
    public class AuthorizationEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("application_id")]
        public Guid? ApplicationId { get; set; }

        [Column("subject")]
        public string? Subject { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Scopes list (stored in JSON format)
        /// </summary>
        [Column("scopes")]
        public string? ScopesJson { get; set; }

        /// <summary>
        /// Extended properties (stored in JSON format)
        /// </summary>
        [Column("properties")]
        public string? PropertiesJson { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [Column("timestamp")]
        [IgnoreUpdate]
        [DefaultSortName]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        [Column("created_at")]
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
