using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Infra.Data.PersistentObjects.IdentityServer;

/// <summary>
/// Represents a property of an identity resource in the database, used for storing additional information or configuration settings related to an identity resource.
/// This class is mapped to the 'identity_resource_properties' table in the database and utilizes Dapper.Contrib for ORM operations.
/// </summary>
[Table("identity_resource_properties")]
public class IdentityResourcePropertyEntity
{
    [Column("identity_resource_id")]
    public Guid IdentityResourceId { get; set; }
    
    [Column("key")]
    public string? Key { get; set; }

    [Column("value")]
    public string? Value { get; set; }
}