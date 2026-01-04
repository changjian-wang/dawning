using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// Role query model
    /// </summary>
    public class RoleModel
    {
        /// <summary>
        /// Role name (fuzzy search)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Display name (fuzzy search)
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Is enabled
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Is system role
        /// </summary>
        public bool? IsSystem { get; set; }
    }
}
