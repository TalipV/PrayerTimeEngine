using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public interface IPrayerTimeCalculator
    {
        public Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(LocalDate date, BaseLocationData locationData, List<GenericSettingConfiguration> configurations, CancellationToken cancellationToken);
        public HashSet<ETimeType> GetUnsupportedTimeTypes();
        public Task<BaseLocationData> GetLocationInfo(CompletePlaceInfo place, CancellationToken cancellationToken);
    }
}