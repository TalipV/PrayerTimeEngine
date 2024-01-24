namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common
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
