using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    [Table("system_configs")]
    public class SystemConfigEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Type: Client, IdentityResource, ApiResource, ApiScope, or parent key for hierarchical linked queries
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Key
        /// </summary>
        [Column("key")]
        public string? Key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [Column("value")]
        public string? Value { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether non-editable by user
        /// </summary>
        [Column("non_editable")]
        public bool NonEditable { get; set; } = true;

        /// <summary>
        /// Timestamp
        /// </summary>
        [Column("timestamp")]
        [DefaultSortName]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Created time
        /// </summary>
        [IgnoreUpdate]
        [Column("created_at")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated_at")]
        public DateTime? Updated { get; set; }
    }
}
