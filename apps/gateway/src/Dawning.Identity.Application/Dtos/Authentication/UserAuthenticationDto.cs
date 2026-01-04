namespace Dawning.Identity.Application.Dtos.Authentication
{
    /// <summary>
    /// User Authentication Model
    /// </summary>
    public class UserAuthenticationDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Role List (loaded from user_roles table)
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Is Enabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is Account Locked Out
        /// </summary>
        public bool IsLockedOut { get; set; } = false;

        /// <summary>
        /// Lockout End Time
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Lockout Message
        /// </summary>
        public string? LockoutMessage { get; set; }
    }
}
