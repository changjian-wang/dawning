using System;
using System.Collections.Generic;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    /// <summary>
    /// API Resource DTO
    /// </summary>
    public class ApiResourceDto
    {
        public Guid? Id { get; set; }

        /// <summary>
        /// Resource Name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display Name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Is Enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Allowed Access Token Signing Algorithms
        /// </summary>
        public List<string> AllowedAccessTokenSigningAlgorithms { get; set; } = new();

        /// <summary>
        /// Show in Discovery Document
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// Associated Scopes
        /// </summary>
        public List<string> Scopes { get; set; } = new();

        /// <summary>
        /// User Claim Types
        /// </summary>
        public List<string> UserClaims { get; set; } = new();

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
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Updated Time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Operator ID (for audit log)
        /// </summary>
        public Guid? OperatorId { get; set; }
    }
}
