using PrayerTimeEngine.Common.Enums;

namespace PrayerTimeEngine.Code.Interfaces
{
    public interface IPrayerTimeCalculator
    {
        public DateTime GetPrayerTimes(DateTime date, EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, ICalculationConfiguration configuration);

        public List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> GetUnsupportedPrayerTimeEvents();
    }
}
