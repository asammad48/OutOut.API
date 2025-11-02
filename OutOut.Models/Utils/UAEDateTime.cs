using Microsoft.Extensions.Options;

namespace OutOut.Models.Utils
{
    public static class UAEDateTime
    {
        private static AppSettings _appSettings;
        public static void InitializeUAEDateTime(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public static DateTime Now
        {
            get
            {
                //var uaeDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"));
                var dateTimeWithOffset = DateTime.UtcNow.Add(new TimeSpan(_appSettings.TimeZoneOffset, 0, 0));
                return new DateTime(dateTimeWithOffset.Ticks, DateTimeKind.Utc);
            }
        }
    }
}
