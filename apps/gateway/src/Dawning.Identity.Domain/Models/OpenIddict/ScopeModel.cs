using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Scope query model
    /// </summary>
    public class ScopeModel
    {
        /// <summary>
        /// Scope name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }
    }
}
