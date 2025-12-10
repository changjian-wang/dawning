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
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public bool Enabled { get; set; } = true;

        public string? AllowedAccessTokenSigningAlgorithms { get; set; }

        public bool ShowInDiscoveryDocument { get; set; } = true;

        public string? Properties { get; set; }

        public long Timestamp { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// API资源作用域关联持久化对象
    /// </summary>
    [Table("api_resource_scopes")]
    public class ApiResourceScopeEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ApiResourceId { get; set; }

        public string Scope { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// API资源声明持久化对象
    /// </summary>
    [Table("api_resource_claims")]
    public class ApiResourceClaimEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ApiResourceId { get; set; }

        public string Type { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
