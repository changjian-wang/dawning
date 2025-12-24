namespace Dawning.Extensions
{
    public static class TimestampUtil
    {
        public static long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
