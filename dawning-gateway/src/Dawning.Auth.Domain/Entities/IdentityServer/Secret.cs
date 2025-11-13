using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// Secret
    /// </summary>
    public abstract class Secret
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("value")]
        public string? Value { get; set; }

        [Column("expiration")]
        public DateTime? Expiration { get; set; }

        [Column("type")]
        public string Type { get; set; } = "SharedSecret";

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

