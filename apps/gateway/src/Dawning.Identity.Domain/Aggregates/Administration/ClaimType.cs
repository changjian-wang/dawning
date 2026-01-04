using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    public class ClaimType : IAggregateRoot
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Type. String, Int, DateTime, Boolean, Enum
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Whether non-editable by user
        /// </summary>
        public bool NonEditable { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}
