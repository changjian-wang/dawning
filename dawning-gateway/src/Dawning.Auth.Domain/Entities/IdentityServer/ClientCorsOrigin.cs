using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_cors_origins")]
    public class ClientCorsOrigin
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("origin")]
        public string? Origin { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

