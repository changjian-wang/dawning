using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class TokenDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Associated Application ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Associated Authorization ID
        /// </summary>
        public Guid? AuthorizationId { get; set; }

        /// <summary>
        /// User Subject
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Token Type (access_token, refresh_token, id_token)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Token Status (valid, revoked, redeemed)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Token Payload (JWT)
        /// </summary>
        public string? Payload { get; set; }

        /// <summary>
        /// Reference ID (for token introspection)
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Expiration Time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

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
