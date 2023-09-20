namespace Algoserver.API.Services
{
    public static class TimeframeHelper {
        public const int DRIVER_GRANULARITY = 1;
        public const int MIN1_GRANULARITY = 60;
        public const int MIN5_GRANULARITY = 300;
        public const int MIN15_GRANULARITY = 900;
        public const int MIN30_GRANULARITY = 1800;
        public const int HOURLY_GRANULARITY = 3600;
        public const int HOUR4_GRANULARITY = 14400;
        public const int DAILY_GRANULARITY = 86400;
        public const int MONTHLY_GRANULARITY = 2592000;
        public const int YEARLY_GRANULARITY = 2592000 * 12;
        public const int YEAR10_GRANULARITY = 2592000 * 12 * 10;
    }
}
