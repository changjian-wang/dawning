using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    public class SystemConfigAggregate : IAggregateRoot
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type: Client, IdentityResource, ApiResource, ApiScope. Can also store parent key to form hierarchical linked queries
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Key
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether non-editable by user
        /// </summary>
        public bool NonEditable { get; set; } = true;

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}
