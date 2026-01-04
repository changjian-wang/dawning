namespace Dawning.Identity.Domain.Models.Gateway
{
    /// <summary>
    /// Gateway route query model
    /// </summary>
    public class GatewayRouteQueryModel
    {
        /// <summary>
        /// Route ID (fuzzy search)
        /// </summary>
        public string? RouteId { get; set; }

        /// <summary>
        /// Route name (fuzzy search)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Associated cluster ID
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// Match path (fuzzy search)
        /// </summary>
        public string? MatchPath { get; set; }

        /// <summary>
        /// Is enabled
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// Keyword (search route ID, name, description, match path)
        /// </summary>
        public string? Keyword { get; set; }
    }
}
