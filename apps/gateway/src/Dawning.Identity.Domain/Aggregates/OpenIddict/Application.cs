using System;
using Dawning.Identity.Domain.Core.Interfaces;
using Dawning.Identity.Domain.Core.Security;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict Application Aggregate Root
    /// </summary>
    public class Application : IAggregateRoot
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client secret (hashed)
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Client type (confidential, public)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Consent type (explicit, implicit, systematic)
        /// </summary>
        public string? ConsentType { get; set; }

        /// <summary>
        /// Permissions list (JSON serialized)
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// Redirect URI list
        /// </summary>
        public List<string> RedirectUris { get; set; } = new();

        /// <summary>
        /// Post-logout redirect URI list
        /// </summary>
        public List<string> PostLogoutRedirectUris { get; set; } = new();

        /// <summary>
        /// Requirements list
        /// </summary>
        public List<string> Requirements { get; set; } = new();

        /// <summary>
        /// Extension properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Business method: Validate client secret
        /// </summary>
        public bool ValidateClientSecret(string secret)
        {
            if (string.IsNullOrEmpty(ClientSecret))
            {
                return false;
            }

            // Verify secret hash using PBKDF2
            return PasswordHasher.Verify(secret, ClientSecret);
        }

        /// <summary>
        /// Business method: Check if has specified permission
        /// </summary>
        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}
