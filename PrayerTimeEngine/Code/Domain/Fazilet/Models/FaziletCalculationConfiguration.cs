using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;

namespace PrayerTimeEngine.Code.Domain.Fazilet.Models
{
    public class FaziletCalculationConfiguration : BaseCalculationConfiguration
    {
        public FaziletCalculationConfiguration(int minuteAdjustment, string countryName, string cityName)
            : base(minuteAdjustment)
        {
            CountryName = PrayerTimesConfigurationStorage.COUNTRY_NAME;
            CityName = PrayerTimesConfigurationStorage.CITY_NAME;
        }

        public override ECalculationSource Source => ECalculationSource.Fazilet;

        public string CountryName { get; set; }
        public string CityName { get; set; }
    }
}
