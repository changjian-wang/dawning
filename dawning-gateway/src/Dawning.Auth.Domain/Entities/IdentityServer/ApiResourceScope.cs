using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("api_resource_scopes")]
    public class ApiResourceScope
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("scope")]
        public string? Scope { get; set; }

        [Column("api_resource_id")]
        public Guid ApiResourceId { get; set; }
    }
}

