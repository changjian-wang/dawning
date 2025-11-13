using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("api_resource_secrets")]
    public class ApiResourceSecret : Secret
    {
        [Column("api_resource_id")]
        public Guid ApiResourceId { get; set; }
    }
}

