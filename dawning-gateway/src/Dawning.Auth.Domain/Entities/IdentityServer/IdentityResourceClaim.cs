using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("identity_resource_claims")]
    public class IdentityResourceClaim : UserClaim
    {
        [Column("identity_resource_id")]
        public Guid IdentityResourceId { get; set; }
    }
}

