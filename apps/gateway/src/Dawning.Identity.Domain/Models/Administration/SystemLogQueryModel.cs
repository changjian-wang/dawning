using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// System log query model
    /// </summary>
    public class SystemLogQueryModel
    {
        /// <summary>
        /// Log level (Info, Warning, Error)
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// Keyword search (fuzzy search in message)
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// Operating user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operating username (fuzzy search)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Request path (fuzzy search)
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
