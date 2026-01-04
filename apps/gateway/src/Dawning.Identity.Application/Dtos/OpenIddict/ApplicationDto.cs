using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class ApplicationDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Client ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Client Type (confidential, public)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Consent Type (explicit, implicit, systematic)
        /// </summary>
        public string? ConsentType { get; set; }

        /// <summary>
        /// Permission List
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// Redirect URI List
        /// </summary>
        public List<string> RedirectUris { get; set; } = new();

        /// <summary>
        /// Post Logout Redirect URI List
        /// </summary>
        public List<string> PostLogoutRedirectUris { get; set; } = new();

        /// <summary>
        /// Requirement List
        /// </summary>
        public List<string> Requirements { get; set; } = new();

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

        /// <summary>
        /// Updated Time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
