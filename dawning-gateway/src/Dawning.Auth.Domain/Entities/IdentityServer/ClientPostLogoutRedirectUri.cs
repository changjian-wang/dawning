using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("client_post_logout_redirect_uris")]
    public class ClientPostLogoutRedirectUri
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("post_logout_redirect_uri")]
        public string? PostLogoutRedirectUri { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }
    }
}

