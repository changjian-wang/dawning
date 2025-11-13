using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Infra.Data.PersistentObjects.IdentityServer;

[Table("identity_resource_claims")]
public class IdentityResourceClaimEntity
{
    [Column("identity_resource_id")]
    public Guid IdentityResourceId { get; set; }
    
    [Column("type")]
    public string? Type { get; set; }
}