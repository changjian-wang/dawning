using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// Represents a claim associated with an API resource in the IdentityServer.
    /// This class extends the UserClaim class, inheriting its properties and adding
    /// a specific reference to the API resource it is associated with via the ApiResourceId property.
    /// </summary>
    /// <remarks>
    /// The ApiResourceClaim class is mapped to the "api_resource_claims" table in the database,
    /// and it is used to store and manage claims that are linked to particular API resources.
    /// </remarks>
    [Table("api_resource_claims")]
    public class ApiResourceClaim : UserClaim
    {
        [Column("api_resource_id")]
        public Guid ApiResourceId { get; set; }
    }
}

