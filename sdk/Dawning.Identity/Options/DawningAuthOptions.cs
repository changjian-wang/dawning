namespace Dawning.Identity.Options;

/// <summary>
/// Dawning authentication configuration options
/// </summary>
public class DawningAuthOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "DawningAuth";

    /// <summary>
    /// Identity server address (for obtaining public key to validate Token)
    /// Example: https://identity.example.com or http://localhost:5202
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (must match Identity service configuration)
    /// Example: https://identity.example.com
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Optional audience validation (leave empty to skip validation)
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Whether to require HTTPS (should be true in production)
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Token expiration clock skew tolerance (minutes)
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Whether to enable role validation
    /// </summary>
    public bool ValidateRoles { get; set; } = true;

    /// <summary>
    /// Whether to enable permission validation
    /// </summary>
    public bool ValidatePermissions { get; set; } = true;

    /// <summary>
    /// Whether to enable tenant validation
    /// </summary>
    public bool ValidateTenant { get; set; } = false;

    /// <summary>
    /// API Gateway address (for inter-service communication)
    /// </summary>
    public string? GatewayUrl { get; set; }
}
