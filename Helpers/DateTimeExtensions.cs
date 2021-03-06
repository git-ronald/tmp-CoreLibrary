using CoreLibrary.ConstantValues;

namespace CoreLibrary.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToStandardFormat(this DateTime? value, bool toLocal = true)
        {
            return value.HasValue ? value.Value.ToStandardFormat(toLocal) : String.Empty;
        }
        public static string ToStandardFormat(this DateTime value, bool toLocal = true)
        {
            DateTime GetOfKind() => toLocal ? value.ToLocalTime() : value;
            return GetOfKind().ToString(CoreConstants.StandardDateTimeFormat);
        }

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

        public static DateTime FindEmptiestPositionInTimeFrame(this IEnumerable<TimeSpan> currentPositions, DateTime start, DateTime end)
        {
            return currentPositions.Select(p => start.Apply(p)).FindEmptiestPositionInTimeFrame(start, end);
        }
        public static DateTime FindEmptiestPositionInTimeFrame(this IEnumerable<DateTime> currentPositions, DateTime start, DateTime end)
        {
            IEnumerable<(DateTime StartTime, TimeSpan TimeSpan)> GetTimeSpans()
            {
                var positions = currentPositions.Where(p => p >= start && p < end).Concat(new DateTime[] { start, end }).OrderBy(t => t).Skip(1);

                DateTime previous = start;
                foreach (DateTime current in positions)
                {
                    yield return (previous, (current - previous));
                    previous = current;
                }
            }

            var (startTime, timeSpan) = GetTimeSpans().MaxBy(x => x.TimeSpan); // Find biggest (== emptiest) time span
            return startTime.AddMilliseconds(timeSpan.TotalMilliseconds / 2);
        }
    }
}
