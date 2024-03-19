namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models
{
    public class CompletePlaceInfo : BasicPlaceInfo
    {
        public CompletePlaceInfo(BasicPlaceInfo basicPlaceInfo)
            : base(basicPlaceInfo)
        {
        }

        public required TimezoneInfo TimezoneInfo { get; set; }
    }
}
