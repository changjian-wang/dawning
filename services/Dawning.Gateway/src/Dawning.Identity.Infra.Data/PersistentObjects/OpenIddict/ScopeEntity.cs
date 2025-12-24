using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// Represents an OpenIddict scope entity, which defines a named scope that can be used to group and manage access
    /// to specific resources in an OpenID Connect or OAuth 2.0 authorization context.
    /// </summary>
    /// <remarks>This entity is typically used to define and manage scopes in an OpenIddict-based
    /// authorization server. Scopes are used to specify the level of access that a client application is requesting for
    /// a resource. Each scope can include additional metadata such as a display name, description, and associated
    /// resources.</remarks>
    [Table("openiddict_scopes")]
    public class ScopeEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("resources")]
        public string? ResourcesJson { get; set; }

        [Column("properties")]
        public string? PropertiesJson { get; set; }

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
