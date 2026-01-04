using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class ScopeDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Scope Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Resource List
        /// </summary>
        public List<string> Resources { get; set; } = new();

        /// <summary>
        /// Extended Properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// Created Time
        /// </summary>
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Operator ID (for audit log)
        /// </summary>
        public Guid? OperatorId { get; set; }
    }
}
