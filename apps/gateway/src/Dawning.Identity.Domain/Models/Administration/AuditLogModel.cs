namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// Audit log query model
    /// </summary>
    public class AuditLogModel
    {
        /// <summary>
        /// Operating user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Operating username (fuzzy search)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Entity type
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Entity ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        public string? IpAddress { get; set; }

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
