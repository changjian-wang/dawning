using System;
using System.Collections.Generic;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// Identity Resource Aggregate Root
    /// Represents user identity information (OpenID Connect)
    /// </summary>
    public class IdentityResource : IAggregateRoot
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Resource name (unique identifier)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether user consent is required
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Whether to emphasize in consent screen
        /// </summary>
        public bool Emphasize { get; set; } = false;

        /// <summary>
        /// Whether to show in discovery document
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// User claim types
        /// </summary>
        public List<string> UserClaims { get; set; } = new();

        /// <summary>
        /// Extension properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
