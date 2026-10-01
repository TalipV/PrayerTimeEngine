using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;

public interface IDynamicPrayerTimeProvider
{
    Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(ZonedDateTime date, BaseLocationData locationData, List<GenericSettingConfiguration> configurations, CancellationToken cancellationToken);
    HashSet<ETimeType> GetUnsupportedTimeTypes();
    Task<BaseLocationData> GetLocationInfo(ProfilePlaceInfo place, CancellationToken cancellationToken);
}