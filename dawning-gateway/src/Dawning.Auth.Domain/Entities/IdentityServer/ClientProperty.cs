using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_properties")]
    public class ClientProperty : Property
    {
        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

