using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Authorization query model
    /// </summary>
    public class AuthorizationModel
    {
        /// <summary>
        /// Associated application ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// User subject
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Authorization type
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Authorization status (valid, revoked)
        /// </summary>
        public string? Status { get; set; }
    }
}
