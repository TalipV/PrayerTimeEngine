using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;

namespace PrayerTimeEngine.Code.DUMMYFOLDER
{
    public class MuwaqqitCalculationConfiguration : ICalculationConfiguration
    {
        public MuwaqqitCalculationConfiguration(decimal longitude, decimal latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public ECalculationSource Source => ECalculationSource.Muwaqqit;

        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
