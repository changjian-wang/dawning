using System.Text.RegularExpressions;

namespace Dawning.Extensions;

/// <summary>
/// 字符串扩展方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 检查字符串是否为 null 或空白
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// 检查字符串是否为 null 或空
    /// </summary>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// 如果字符串为 null 或空白，返回默认值
    /// </summary>
    public static string IfNullOrWhiteSpace(this string? value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value;

    /// <summary>
    /// 截断字符串到指定长度，超出部分用省略号代替
    /// </summary>
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// 将字符串转换为 PascalCase
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
    /// 将字符串转换为 camelCase
    /// </summary>
    public static string ToCamelCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // 如果已经是 PascalCase（首字母大写），直接处理
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
    /// 将字符串转换为 snake_case
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
    /// 将字符串转换为 kebab-case
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
    /// 移除字符串中的所有空白字符
    /// </summary>
    public static string RemoveWhitespace(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Regex.Replace(value, @"\s+", string.Empty);
    }

    /// <summary>
    /// 安全地获取子字符串，不会抛出异常
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
    /// 检查字符串是否为有效的邮箱格式
    /// </summary>
    public static bool IsValidEmail(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 检查字符串是否为有效的手机号格式（中国大陆）
    /// </summary>
    public static bool IsValidPhoneNumber(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^1[3-9]\d{9}$");
    }

    /// <summary>
    /// 对字符串进行掩码处理（用于隐私保护）
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
