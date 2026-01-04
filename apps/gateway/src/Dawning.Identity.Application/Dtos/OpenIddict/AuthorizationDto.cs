using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class AuthorizationDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Associated Application ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// User Subject
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Authorization Type
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Authorization Status (valid, revoked)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Authorized Scope List
        /// </summary>
        public List<string> Scopes { get; set; } = new();

        /// <summary>
        /// Extended Properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Timestamp
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// Created Time
        /// </summary>
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
