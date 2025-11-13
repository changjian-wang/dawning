using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_scopes")]
    public class ClientScope
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("scope")]
        public string? Scope { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

