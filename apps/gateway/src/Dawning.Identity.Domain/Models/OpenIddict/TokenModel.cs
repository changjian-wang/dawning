using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Token query model
    /// </summary>
    public class TokenModel
    {
        /// <summary>
        /// Associated application ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Associated authorization ID
        /// </summary>
        public Guid? AuthorizationId { get; set; }

        /// <summary>
        /// User subject
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Token type (access_token, refresh_token, id_token)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Token status (valid, revoked, redeemed)
        /// </summary>
        public string? Status { get; set; }
    }
}
