using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public interface IPrayerTimeCalculator
    {
        public Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(LocalDate date, BaseLocationData locationData, List<GenericSettingConfiguration> configurations);
        public HashSet<ETimeType> GetUnsupportedTimeTypes();
        public Task<BaseLocationData> GetLocationInfo(CompletePlaceInfo place);
    }
}