using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    [Table("claim_types")]
    public class ClaimTypeEntity
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Type. String, Int, DateTime, Boolean, Enum
        /// </summary>
        [Column("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether required
        /// </summary>
        [Column("required")]
        public bool Required { get; set; } = false;

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
        [Column("created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        [Column("updated")]
        public DateTime? Updated { get; set; }
    }
}
