using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_grant_types")]
    public class ClientGrantType
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("grant_type")]
        public string? GrantType { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

