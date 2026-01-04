using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// Permission query model
    /// </summary>
    public class PermissionModel
    {
        /// <summary>
        /// Permission code (fuzzy search)
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Permission name (fuzzy search)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Resource type
        /// </summary>
        public string? Resource { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Permission category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Is enabled
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
