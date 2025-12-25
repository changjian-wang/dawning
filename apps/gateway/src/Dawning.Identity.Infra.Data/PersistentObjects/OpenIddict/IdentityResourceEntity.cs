using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// 身份资源持久化对象
    /// </summary>
    [Table("identity_resources")]
    public class IdentityResourceEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("enabled")]
        public bool Enabled { get; set; } = true;

        [Column("required")]
        public bool Required { get; set; } = false;

        [Column("emphasize")]
        public bool Emphasize { get; set; } = false;

        [Column("show_in_discovery_document")]
        public bool ShowInDiscoveryDocument { get; set; } = true;

        [Column("properties")]
        public string? Properties { get; set; }

        [Column("timestamp")]
        [DefaultSortName]
        public long Timestamp { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 身份资源声明持久化对象
    /// </summary>
    [Table("identity_resource_claims")]
    public class IdentityResourceClaimEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("identity_resource_id")]
        public Guid IdentityResourceId { get; set; }

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
