namespace Dawning.Identity.Domain.Models.Gateway
{
    /// <summary>
    /// Gateway cluster query model
    /// </summary>
    public class GatewayClusterQueryModel
    {
        /// <summary>
        /// Cluster ID (fuzzy search)
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// Cluster name (fuzzy search)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Load balancing policy
        /// </summary>
        public string? LoadBalancingPolicy { get; set; }

        /// <summary>
        /// Is enabled
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// Keyword (search cluster ID, name, description)
        /// </summary>
        public string? Keyword { get; set; }
    }
}
