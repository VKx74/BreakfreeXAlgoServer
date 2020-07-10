using System;

namespace Common.Logic.Helpers
{
    public class BaseHelper
    {
        public static DateTime EpochTimeToDateTime(long epochTime) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(epochTime);

        public static long GetEpochTime() => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        public static long DateTimeToUnixEpochTime(DateTime date) => (long)Math.Round((date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
    }
}
