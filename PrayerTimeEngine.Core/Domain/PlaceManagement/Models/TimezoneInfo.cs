namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models
{
    public class TimezoneInfo
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public int UtcOffsetSeconds { get; set; }
    }
}
