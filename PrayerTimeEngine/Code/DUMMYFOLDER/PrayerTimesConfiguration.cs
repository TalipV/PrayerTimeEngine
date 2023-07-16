using PrayerTimeEngine.Code.Domain.Models;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Common.Enums;

namespace PrayerTimeEngine.Code.DUMMYFOLDER
{
    public class PrayerTimesConfiguration
    {
        public static PrayerTimesConfiguration Instance = new PrayerTimesConfiguration();

        public PrayerTimeLocation Location { get; set; }

        private const decimal INNSBRUCK_LATITUDE = 47.2803835M;
        private const decimal INNSBRUCK_LONGITUDE = 11.41337M;

        public Dictionary<(EPrayerTime, EPrayerTimeEvent), ICalculationConfiguration> Configurations { get; } =
            new Dictionary<(EPrayerTime, EPrayerTimeEvent), ICalculationConfiguration>
            {
                [(EPrayerTime.Fajr, EPrayerTimeEvent.Start)] = new MuwaqqitCalculationConfigurationFajrIshaStart(INNSBRUCK_LONGITUDE, INNSBRUCK_LATITUDE, -12.0M),
                [(EPrayerTime.Fajr, EPrayerTimeEvent.End)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),

                [(EPrayerTime.Duha, EPrayerTimeEvent.Start)] = null,
                [(EPrayerTime.Duha, EPrayerTimeEvent.End)] = null,

                [(EPrayerTime.Dhuhr, EPrayerTimeEvent.Start)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),
                [(EPrayerTime.Dhuhr, EPrayerTimeEvent.End)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),

                [(EPrayerTime.Asr, EPrayerTimeEvent.Start)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),
                [(EPrayerTime.Asr, EPrayerTimeEvent.End)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),

                [(EPrayerTime.Maghrib, EPrayerTimeEvent.Start)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),
                [(EPrayerTime.Maghrib, EPrayerTimeEvent.End)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),

                [(EPrayerTime.Isha, EPrayerTimeEvent.Start)] = new FaziletCalculationConfiguration("Avusturya", "Innsbruck"),
                [(EPrayerTime.Isha, EPrayerTimeEvent.End)] = null,
            };

        private PrayerTimesConfiguration() { }
    }
}
