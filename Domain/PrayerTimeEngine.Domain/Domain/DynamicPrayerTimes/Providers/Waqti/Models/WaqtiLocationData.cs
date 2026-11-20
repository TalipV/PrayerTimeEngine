using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;

public class WaqtiLocationData : BaseLocationData
{
    public required decimal Longitude { get; init; }
    public required decimal Latitude { get; init; }
    public required string TimezoneName { get; set; }
    public override EDynamicPrayerTimeProviderType Source => EDynamicPrayerTimeProviderType.Waqti;

    #region System.Object overrides

    public override bool Equals(object? obj)
    {
        if (obj is not WaqtiLocationData otherLocationData)
        {
            return false;
        }

        return Longitude == otherLocationData.Longitude
            && Latitude == otherLocationData.Latitude
            && TimezoneName == otherLocationData.TimezoneName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Longitude, Latitude, TimezoneName);
    }

    public override string ToString()
    {
        return $"{Source}: {TimezoneName}, {Latitude}°/{Longitude}°";
    }

    #endregion System.Object overrides
}
