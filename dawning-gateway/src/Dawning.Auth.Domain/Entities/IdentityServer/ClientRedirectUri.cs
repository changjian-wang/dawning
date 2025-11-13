using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_redirect_uris")]
    public class ClientRedirectUri
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("redirect_uri")]
        public string? RedirectUri { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

