using PrayerTimeEngine.Common.Enum;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    public class GenericSettingConfiguration
    {
        public GenericSettingConfiguration(ETimeType timeType, int minuteAdjustment = 0, ECalculationSource calculationSource = ECalculationSource.None, bool isTimeShown = true)
        {
            MinuteAdjustment = minuteAdjustment;
            IsTimeShown = isTimeShown;
            TimeType = timeType;
            Source = calculationSource;
        }

        public virtual ECalculationSource Source { get; init; }
        public ETimeType TimeType { get; init; }
        public int MinuteAdjustment { get; set; }
        public bool IsTimeShown { get; set; }
    }
}
