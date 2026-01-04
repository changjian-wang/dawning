namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Identity resource query model
    /// </summary>
    public class IdentityResourceModel
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
        /// Is consent required
        /// </summary>
        public bool? Required { get; set; }
    }
}
