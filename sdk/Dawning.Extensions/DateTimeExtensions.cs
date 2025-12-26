namespace Dawning.Extensions;

/// <summary>
/// 日期时间扩展方法
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 获取当天的开始时间 (00:00:00)
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime) => dateTime.Date;

    /// <summary>
    /// 获取当天的结束时间 (23:59:59.999)
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime) =>
        dateTime.Date.AddDays(1).AddMilliseconds(-1);

    /// <summary>
    /// 获取本周的第一天（周一）
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
    /// 获取本周的最后一天
    /// </summary>
    public static DateTime EndOfWeek(
        this DateTime dateTime,
        DayOfWeek startOfWeek = DayOfWeek.Monday
    ) => dateTime.StartOfWeek(startOfWeek).AddDays(6).EndOfDay();

    /// <summary>
    /// 获取本月的第一天
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime) =>
        new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// 获取本月的最后一天
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime) =>
        dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);

    /// <summary>
    /// 获取本年的第一天
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime) =>
        new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// 获取本年的最后一天
    /// </summary>
    public static DateTime EndOfYear(this DateTime dateTime) =>
        new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999, dateTime.Kind);

    /// <summary>
    /// 判断是否是周末
    /// </summary>
    public static bool IsWeekend(this DateTime dateTime) =>
        dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// 判断是否是工作日
    /// </summary>
    public static bool IsWeekday(this DateTime dateTime) => !dateTime.IsWeekend();

    /// <summary>
    /// 获取两个日期之间的天数
    /// </summary>
    public static int DaysBetween(this DateTime startDate, DateTime endDate) =>
        (int)(endDate.Date - startDate.Date).TotalDays;

    /// <summary>
    /// 计算年龄
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
    /// 转换为 Unix 时间戳（秒）
    /// </summary>
    public static long ToUnixTimeSeconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();

    /// <summary>
    /// 转换为 Unix 时间戳（毫秒）
    /// </summary>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();

    /// <summary>
    /// 从 Unix 时间戳（秒）创建 DateTime
    /// </summary>
    public static DateTime FromUnixTimeSeconds(this long timestamp) =>
        DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;

    /// <summary>
    /// 从 Unix 时间戳（毫秒）创建 DateTime
    /// </summary>
    public static DateTime FromUnixTimeMilliseconds(this long timestamp) =>
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;

    /// <summary>
    /// 获取相对时间描述（如"3分钟前"）
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var now = DateTime.Now;
        var span = now - dateTime;

        if (span.TotalSeconds < 60)
            return "刚刚";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes}分钟前";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours}小时前";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays}天前";
        if (span.TotalDays < 30)
            return $"{(int)(span.TotalDays / 7)}周前";
        if (span.TotalDays < 365)
            return $"{(int)(span.TotalDays / 30)}个月前";

        return $"{(int)(span.TotalDays / 365)}年前";
    }

    /// <summary>
    /// 格式化为标准日期字符串 (yyyy-MM-dd)
    /// </summary>
    public static string ToDateString(this DateTime dateTime) => dateTime.ToString("yyyy-MM-dd");

    /// <summary>
    /// 格式化为标准日期时间字符串 (yyyy-MM-dd HH:mm:ss)
    /// </summary>
    public static string ToDateTimeString(this DateTime dateTime) =>
        dateTime.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// 格式化为 ISO 8601 格式
    /// </summary>
    public static string ToIso8601(this DateTime dateTime) =>
        dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    /// <summary>
    /// 设置时间部分
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
    /// 添加工作日
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
