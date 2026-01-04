namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// API resource query model
    /// </summary>
    public class ApiResourceModel
    {
        /// <summary>
        /// Resource name (fuzzy match)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Display name (fuzzy match)
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Is enabled
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// Associated scope (exact match)
        /// </summary>
        public string? Scope { get; set; }
    }
}
