using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict Authorization Aggregate Root
    /// </summary>
    public class Authorization : IAggregateRoot
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Associated application ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// User subject identifier
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Authorization type
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Authorization status (valid, revoked)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Authorized scopes list
        /// </summary>
        public List<string> Scopes { get; set; } = new();

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
