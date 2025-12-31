namespace Dawning.Extensions;

/// <summary>
/// DateTime extension methods
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Get the start of day (00:00:00)
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime) => dateTime.Date;

    /// <summary>
    /// Get the end of day (23:59:59.999)
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime) =>
        dateTime.Date.AddDays(1).AddMilliseconds(-1);

    /// <summary>
    /// Get the first day of the week (Monday)
    /// </summary>
    public static DateTime StartOfWeek(
        this DateTime dateTime,
        DayOfWeek startOfWeek = DayOfWeek.Monday
    )
    {
        var diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        return dateTime.AddDays(-diff).Date;
    }

    /// <summary>
    /// Get the last day of the week
    /// </summary>
    public static DateTime EndOfWeek(
        this DateTime dateTime,
        DayOfWeek startOfWeek = DayOfWeek.Monday
    ) => dateTime.StartOfWeek(startOfWeek).AddDays(6).EndOfDay();

    /// <summary>
    /// Get the first day of the month
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime) =>
        new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Get the last day of the month
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime) =>
        dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);

    /// <summary>
    /// Get the first day of the year
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime) =>
        new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Get the last day of the year
    /// </summary>
    public static DateTime EndOfYear(this DateTime dateTime) =>
        new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999, dateTime.Kind);

    /// <summary>
    /// Check if the date is a weekend
    /// </summary>
    public static bool IsWeekend(this DateTime dateTime) =>
        dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// Check if the date is a weekday
    /// </summary>
    public static bool IsWeekday(this DateTime dateTime) => !dateTime.IsWeekend();

    /// <summary>
    /// Get the number of days between two dates
    /// </summary>
    public static int DaysBetween(this DateTime startDate, DateTime endDate) =>
        (int)(endDate.Date - startDate.Date).TotalDays;

    /// <summary>
    /// Calculate age
    /// </summary>
    public static int CalculateAge(this DateTime birthDate, DateTime? referenceDate = null)
    {
        var reference = referenceDate ?? DateTime.Today;
        var age = reference.Year - birthDate.Year;

        if (birthDate.Date > reference.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Convert to Unix timestamp (seconds)
    /// </summary>
    public static long ToUnixTimeSeconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();

    /// <summary>
    /// Convert to Unix timestamp (milliseconds)
    /// </summary>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();

    /// <summary>
    /// Create DateTime from Unix timestamp (seconds)
    /// </summary>
    public static DateTime FromUnixTimeSeconds(this long timestamp) =>
        DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;

    /// <summary>
    /// Create DateTime from Unix timestamp (milliseconds)
    /// </summary>
    public static DateTime FromUnixTimeMilliseconds(this long timestamp) =>
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;

    /// <summary>
    /// Get relative time description (e.g., "3 minutes ago")
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var now = DateTime.Now;
        var span = now - dateTime;

        if (span.TotalSeconds < 60)
            return "just now";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} minutes ago";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} hours ago";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} days ago";
        if (span.TotalDays < 30)
            return $"{(int)(span.TotalDays / 7)} weeks ago";
        if (span.TotalDays < 365)
            return $"{(int)(span.TotalDays / 30)} months ago";

        return $"{(int)(span.TotalDays / 365)} years ago";
    }

    /// <summary>
    /// Format as standard date string (yyyy-MM-dd)
    /// </summary>
    public static string ToDateString(this DateTime dateTime) => dateTime.ToString("yyyy-MM-dd");

    /// <summary>
    /// Format as standard datetime string (yyyy-MM-dd HH:mm:ss)
    /// </summary>
    public static string ToDateTimeString(this DateTime dateTime) =>
        dateTime.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Format as ISO 8601 format
    /// </summary>
    public static string ToIso8601(this DateTime dateTime) =>
        dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    /// <summary>
    /// Set time portion
    /// </summary>
    public static DateTime SetTime(
        this DateTime dateTime,
        int hour,
        int minute = 0,
        int second = 0,
        int millisecond = 0
    ) =>
        new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            hour,
            minute,
            second,
            millisecond,
            dateTime.Kind
        );

    /// <summary>
    /// Add business days
    /// </summary>
    public static DateTime AddWorkdays(this DateTime dateTime, int days)
    {
        var direction = days < 0 ? -1 : 1;
        var remaining = Math.Abs(days);
        var result = dateTime;

        while (remaining > 0)
        {
            result = result.AddDays(direction);
            if (result.IsWeekday())
                remaining--;
        }

        return result;
    }
}
