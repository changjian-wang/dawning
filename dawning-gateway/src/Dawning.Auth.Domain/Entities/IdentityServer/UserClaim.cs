using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// User Claim
    /// </summary>
    public abstract class UserClaim
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("type")]
        public string? Type { get; set; }
    }
}

