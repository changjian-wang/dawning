using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict Scope Aggregate Root
    /// </summary>
    public class Scope : IAggregateRoot
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Scope name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Resources list
        /// </summary>
        public List<string> Resources { get; set; } = new();

        /// <summary>
        /// Extension properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
