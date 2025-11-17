using System;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
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

        [Column("created_at")]
        [IgnoreUpdate]
        [DefaultSortName]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

