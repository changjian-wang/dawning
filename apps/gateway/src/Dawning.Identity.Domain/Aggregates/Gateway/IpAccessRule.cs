using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// IP Access Rule Aggregate Root
    /// </summary>
    public class IpAccessRule : IAggregateRoot
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// IP address or CIDR range (e.g., 192.168.1.0/24)
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Rule type: whitelist, blacklist
        /// </summary>
        public string RuleType { get; set; } = "blacklist";

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Expiration time (optional, used for temporary bans)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Created by
        /// </summary>
        public string? CreatedBy { get; set; }
    }
}
