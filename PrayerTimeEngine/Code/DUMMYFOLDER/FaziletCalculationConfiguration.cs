using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;

namespace PrayerTimeEngine.Code.DUMMYFOLDER
{
    public class FaziletCalculationConfiguration : ICalculationConfiguration
    {
        public FaziletCalculationConfiguration(string countryName, string cityName)
        {
            CountryName = countryName;
            CityName = cityName;
        }

        public ECalculationSource Source => ECalculationSource.Fazilet;

        public string CountryName { get; set; }
        public string CityName { get; set; }
    }
}
