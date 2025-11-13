using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_secrets")]
    public class ClientSecret : Secret
    {
        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

