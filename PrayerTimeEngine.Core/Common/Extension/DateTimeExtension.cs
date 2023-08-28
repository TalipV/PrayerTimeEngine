namespace PrayerTimeEngine.Core.Common.Extension
{
    public static class DateTimeExtension
    {
        public static DateTime WithoutFractionsOfSeconds(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
    }
}
