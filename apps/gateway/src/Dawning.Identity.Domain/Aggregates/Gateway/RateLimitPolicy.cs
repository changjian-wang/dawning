using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// Rate Limit Policy Aggregate Root
    /// </summary>
    public class RateLimitPolicy : IAggregateRoot
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Policy name (unique identifier)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Policy type: fixed-window, sliding-window, token-bucket, concurrency
        /// </summary>
        public string PolicyType { get; set; } = "fixed-window";

        /// <summary>
        /// Number of requests allowed per window
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Time window (in seconds)
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Number of segments per sliding window (only for sliding-window)
        /// </summary>
        public int SegmentsPerWindow { get; set; } = 6;

        /// <summary>
        /// Queue limit
        /// </summary>
        public int QueueLimit { get; set; } = 0;

        /// <summary>
        /// Token replenishment rate (only for token-bucket)
        /// </summary>
        public int TokensPerPeriod { get; set; } = 10;

        /// <summary>
        /// Token replenishment period in seconds (only for token-bucket)
        /// </summary>
        public int ReplenishmentPeriodSeconds { get; set; } = 1;

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
