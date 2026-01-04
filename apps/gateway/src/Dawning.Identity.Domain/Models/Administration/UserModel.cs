using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// User query filter model
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Username (fuzzy search)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Email (fuzzy search)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Display name (fuzzy search)
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Role filter
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
