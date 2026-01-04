namespace Dawning.Identity.Application.Dtos.User
{
    /// <summary>
    /// User Info Response DTO
    /// </summary>
    public class UserInfoDto
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
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Role List
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Display Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Avatar URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Created Time
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
