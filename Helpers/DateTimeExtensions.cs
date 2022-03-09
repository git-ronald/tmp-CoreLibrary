namespace CoreLibrary.Helpers
{
    public static class DateTimeExtensions
    {
        public static TimeSpan CalcDeltaWithLastTimeCompartment(this DateTime start, int compartmentMinutes)
        {
            DateTime utcStart = start.ToUniversalTime();
            DateTime lastMoment = utcStart.CalcLastNthMinute(compartmentMinutes);
            return utcStart - lastMoment;
        }

        /// <summary>
        /// Example: interval = 5.
        /// If startTime = 12:04 then result = 12:05
        /// If startTime = 12:05 then result = 12:10
        /// If startTime = 12:56 then result = 13:00
        /// </summary>
        public static DateTime CalcNextNthMinute(this DateTime start, int minutesInterval)
        {
            DateTime utcStart = start.ToUniversalTime();
            int addedHour = 0;
            int nextMinute = ((utcStart.Minute / minutesInterval) + 1) * minutesInterval;
            if (nextMinute > 59)
            {
                nextMinute %= 60;
                addedHour = 1;
            }

            return new DateTime(utcStart.Year, utcStart.Month, utcStart.Day, utcStart.Hour, nextMinute, 0, DateTimeKind.Utc).AddHours(addedHour);
        }

        public static DateTime CalcLastNthMinute(this DateTime start, int minutesInterval)
        {
            DateTime utcStart = start.ToUniversalTime();
            int lastMinute = (utcStart.Minute / minutesInterval) * minutesInterval;
            return new DateTime(utcStart.Year, utcStart.Month, utcStart.Day, utcStart.Hour, lastMinute, 0, DateTimeKind.Utc);
        }

        public static DateTime Apply(this DateTime date, TimeSpan time, int? seconds = null, bool alwaysInTheFuture = true)
        {
            DateTime result = new(date.Year, date.Month, date.Day, time.Hours, time.Minutes, seconds ?? time.Seconds, date.Kind);
            if (alwaysInTheFuture && result <= date)
            {
                return result.AddDays(1);
            }
            return result;
        }
    }
}
