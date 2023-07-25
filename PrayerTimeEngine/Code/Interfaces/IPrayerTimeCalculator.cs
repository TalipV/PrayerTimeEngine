using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Code.Interfaces
{
    public interface IPrayerTimeCalculator
    {
        public Task<DateTime> GetPrayerTimesAsync(
            DateTime date, 
            EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, 
            BaseCalculationConfiguration configuration);

        public List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> GetUnsupportedPrayerTimeEvents();
    }
}
