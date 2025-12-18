using System;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// API资源持久化对象
    /// </summary>
    [Table("api_resources")]
    public class ApiResourceEntity
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

        [Column("allowed_access_token_signing_algorithms")]
        public string? AllowedAccessTokenSigningAlgorithms { get; set; }

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
    /// API资源作用域关联持久化对象
    /// </summary>
    [Table("api_resource_scopes")]
    public class ApiResourceScopeEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("api_resource_id")]
        public Guid ApiResourceId { get; set; }

        [Column("scope")]
        public string Scope { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// API资源声明持久化对象
    /// </summary>
    [Table("api_resource_claims")]
    public class ApiResourceClaimEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("api_resource_id")]
        public Guid ApiResourceId { get; set; }

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
