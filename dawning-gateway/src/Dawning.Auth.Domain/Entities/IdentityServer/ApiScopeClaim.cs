using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("api_scope_claims")]
    public class ApiScopeClaim : UserClaim
    {
        [Column("scope_id")]
        public Guid ScopeId { get; set; }
    }
}

