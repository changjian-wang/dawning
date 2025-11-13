using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("identity_resource_properties")]
    public class IdentityResourceProperty : Property
    {
        [Column("identity_resource_id")]
        public Guid IdentityResourceId { get; set; }
    }
}

