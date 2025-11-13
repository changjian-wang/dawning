using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_id_p_restrictions")]
    public class ClientIdPRestriction
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("provider")]
        public string? Provider { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

