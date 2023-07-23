using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Models
{
    public class MuwaqqitCalculationConfiguration : BaseCalculationConfiguration
    {
        public MuwaqqitCalculationConfiguration(int minuteAdjustment, decimal longitude, decimal latitude, string timezone) 
            : base(minuteAdjustment)
        {
            Longitude = PrayerTimesConfigurationStorage.INNSBRUCK_LONGITUDE;
            Latitude = PrayerTimesConfigurationStorage.INNSBRUCK_LATITUDE;
            Timezone = PrayerTimesConfigurationStorage.TIMEZONE;
        }
        public override ECalculationSource Source => ECalculationSource.Muwaqqit;

        public string Timezone { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
