namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models
{
    internal class LocationIQTimezone
    {
        public string name { get; set; }
        public int now_in_dst { get; set; }
        public int offset_sec { get; set; }
        public string short_name { get; set; }
    }
}
