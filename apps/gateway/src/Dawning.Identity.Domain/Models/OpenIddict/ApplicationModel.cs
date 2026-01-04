using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Application query model
    /// </summary>
    public class ApplicationModel
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Client type (confidential, public)
        /// </summary>
        public string? Type { get; set; }
    }
}
