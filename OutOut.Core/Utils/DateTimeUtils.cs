using OutOut.Models.Models;

namespace OutOut.Core.Utils
{
    public static class DateTimeUtils
    {
        private static bool IsInRangeOf(this AvailableTime availableTime, TimeSpan requestedTime)
        {
            var effectiveFrom = availableTime.From.TrimSeconds();
            var effectiveTo = availableTime.To.TrimSeconds();
            requestedTime = requestedTime.TrimSeconds();

            return effectiveFrom <= requestedTime && effectiveTo > requestedTime;
        }

        public static bool IsInRangeOf(this AvailableTime availableTime, DateTime requestedDateTime, DateTime? afterThisDate = null)
        {
            var hasPassedCurrentDate = false;
            if (afterThisDate != null)
            {
                var effectiveAfterThisDate = afterThisDate?.TrimSeconds();
                var effectiveRequestedDateTime = requestedDateTime.TrimSeconds();

                hasPassedCurrentDate = effectiveRequestedDateTime <= effectiveAfterThisDate;
            }

            return availableTime.IsInRangeOf(requestedDateTime.TimeOfDay)
                && availableTime.Days.Contains(requestedDateTime.Date.DayOfWeek)
                && !hasPassedCurrentDate;
        }

        public static bool IsInRangeOf(this List<AvailableTime> availableTimes, DateTime requestedDateTime, DateTime? afterThisDate = null)
        {
            var isAvailable = availableTimes.Select(time => IsInRangeOf(time, requestedDateTime, afterThisDate));
            return isAvailable.Any(available => available);
        }

        public static TimeSpan TrimSeconds(this TimeSpan time) => new TimeSpan(time.Hours, time.Minutes, 0);

        public static DateTime TrimSeconds(this DateTime dateTime) => dateTime.Subtract(new TimeSpan(0, 0, dateTime.Second));
    }
}
