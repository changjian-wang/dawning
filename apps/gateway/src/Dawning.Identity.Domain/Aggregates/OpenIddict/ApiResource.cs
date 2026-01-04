using System;
using System.Collections.Generic;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// API Resource Aggregate Root
    /// Represents a protected API resource that can be accessed using tokens
    /// </summary>
    public class ApiResource : IAggregateRoot
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
        /// Allowed access token signing algorithms
        /// </summary>
        public List<string> AllowedAccessTokenSigningAlgorithms { get; set; } = new();

        /// <summary>
        /// Whether to show in discovery document
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// Associated scopes
        /// </summary>
        public List<string> Scopes { get; set; } = new();

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
