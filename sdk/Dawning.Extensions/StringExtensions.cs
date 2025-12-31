using System.Text.RegularExpressions;

namespace Dawning.Extensions;

/// <summary>
/// String extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if the string is null or whitespace
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Checks if the string is null or empty
    /// </summary>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns the default value if the string is null or whitespace
    /// </summary>
    public static string IfNullOrWhiteSpace(this string? value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value;

    /// <summary>
    /// Truncates the string to the specified length, replacing excess with ellipsis
    /// </summary>
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts the string to PascalCase
    /// </summary>
    public static string ToPascalCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var words = Regex.Split(value, @"[\s_\-]+");
        return string.Concat(
            words.Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant())
        );
    }

    /// <summary>
    /// Converts the string to camelCase
    /// </summary>
    public static string ToCamelCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // If already PascalCase (starts with uppercase), handle directly
        if (
            char.IsUpper(value[0])
            && !value.Contains('_')
            && !value.Contains('-')
            && !value.Contains(' ')
        )
        {
            return char.ToLowerInvariant(value[0]) + value[1..];
        }

        var pascal = value.ToPascalCase();
        if (string.IsNullOrEmpty(pascal))
            return string.Empty;

        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    /// <summary>
    /// Converts the string to snake_case
    /// </summary>
    public static string ToSnakeCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex
            .Replace(value, @"([a-z])([A-Z])", "$1_$2")
            .Replace(" ", "_")
            .Replace("-", "_")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Converts the string to kebab-case
    /// </summary>
    public static string ToKebabCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex
            .Replace(value, @"([a-z])([A-Z])", "$1-$2")
            .Replace(" ", "-")
            .Replace("_", "-")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Removes all whitespace characters from the string
    /// </summary>
    public static string RemoveWhitespace(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Regex.Replace(value, @"\s+", string.Empty);
    }

    /// <summary>
    /// Safely gets a substring without throwing exceptions
    /// </summary>
    public static string SafeSubstring(this string? value, int startIndex, int? length = null)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (startIndex < 0 || startIndex >= value.Length)
            return string.Empty;

        var maxLength = value.Length - startIndex;
        var actualLength = length.HasValue ? Math.Min(length.Value, maxLength) : maxLength;

        return value.Substring(startIndex, actualLength);
    }

    /// <summary>
    /// Checks if the string is a valid email format
    /// </summary>
    public static bool IsValidEmail(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Checks if the string is a valid phone number format (China mainland)
    /// </summary>
    public static bool IsValidPhoneNumber(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^1[3-9]\d{9}$");
    }

    /// <summary>
    /// Masks the string (for privacy protection)
    /// </summary>
    public static string Mask(
        this string? value,
        int visibleStart = 3,
        int visibleEnd = 4,
        char maskChar = '*'
    )
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length <= visibleStart + visibleEnd)
            return new string(maskChar, value.Length);

        var start = value[..visibleStart];
        var end = value[^visibleEnd..];
        var masked = new string(maskChar, value.Length - visibleStart - visibleEnd);

        return start + masked + end;
    }
}
