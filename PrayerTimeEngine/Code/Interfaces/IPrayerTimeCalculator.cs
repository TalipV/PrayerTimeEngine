using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Domain.Model;

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
