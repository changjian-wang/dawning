using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_claims")]
    public class ClientClaim
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("value")]
        public string? Value { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

