using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.LocationService.Models;
using PrayerTimeEngine.Domain.Model;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand
{
    public interface IPrayerTimeService
    {
        public Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(DateTime date, BaseLocationData locationData, List<GenericSettingConfiguration> configurations);
        public HashSet<ETimeType> GetUnsupportedTimeTypes();
        public Task<BaseLocationData> GetLocationInfo(LocationIQPlace place);
    }
}