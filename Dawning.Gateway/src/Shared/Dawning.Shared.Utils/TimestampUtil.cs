namespace Dawning.Shared.Utils
{
    public static class TimestampUtil
    {
        public static long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
