using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    [Table("persisted_grants")]
    public class PersistedGrant
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("key")]
        public string? Key { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("subject_id")]
        public string? SubjectId { get; set; }

        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("client_id")]
        public string? ClientId { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("creation_time")]
        public DateTime CreationTime { get; set; }

        [Column("expiration")]
        public DateTime? Expiration { get; set; }

        [Column("consumed_time")]
        public DateTime? ConsumedTime { get; set; }

        [Column("data")]
        public string? Data { get; set; }
    }
}

