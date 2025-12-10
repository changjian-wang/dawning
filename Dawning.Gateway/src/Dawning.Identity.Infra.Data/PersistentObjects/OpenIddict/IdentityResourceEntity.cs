using System;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// 身份资源持久化对象
    /// </summary>
    [Table("identity_resources")]
    public class IdentityResourceEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Required { get; set; } = false;

        public bool Emphasize { get; set; } = false;

        public bool ShowInDiscoveryDocument { get; set; } = true;

        public string? Properties { get; set; }

        public long Timestamp { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 身份资源声明持久化对象
    /// </summary>
    [Table("identity_resource_claims")]
    public class IdentityResourceClaimEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid IdentityResourceId { get; set; }

        public string Type { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
