using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict Token Aggregate Root
    /// </summary>
    public class Token : IAggregateRoot
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Associated application ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Associated authorization ID
        /// </summary>
        public Guid? AuthorizationId { get; set; }

        /// <summary>
        /// User subject identifier
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Token type (access_token, refresh_token, id_token)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Token status (valid, revoked, redeemed)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Token payload (JWT)
        /// </summary>
        public string? Payload { get; set; }

        /// <summary>
        /// Reference ID (used for token introspection)
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Expiration time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Expiration time (OpenIddict naming convention)
        /// </summary>
        public DateTime? ExpirationDate
        {
            get => ExpiresAt;
            set => ExpiresAt = value;
        }

        /// <summary>
        /// Redemption time (when the token was used)
        /// </summary>
        public DateTime? RedemptionDate { get; set; }

        /// <summary>
        /// Extension properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Business method: Check if token is expired
        /// </summary>
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        }
    }
}
