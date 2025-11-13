using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// Property
    /// </summary>
    public abstract class Property
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("key")]
        public string? Key { get; set; }

        [Column("value")]
        public string? Value { get; set; }
    }
}

