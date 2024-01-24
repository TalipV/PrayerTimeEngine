using System.Text.Json.Serialization;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Core.Domain.Models
{
    [JsonDerivedType(typeof(GenericSettingConfiguration), typeDiscriminator: "GenericConfig")]
    [JsonDerivedType(typeof(MuwaqqitDegreeCalculationConfiguration), typeDiscriminator: "MuwaqqitDegreeConfig")]
    public class GenericSettingConfiguration
    {
        public required ETimeType TimeType { get; init; }
        public virtual ECalculationSource Source { get; init; } = ECalculationSource.None;
        public int MinuteAdjustment { get; set; } = 0;
        public bool IsTimeShown { get; set; } = true;

        public override bool Equals(object obj)
        {
            if (obj is not GenericSettingConfiguration otherSettingConfig)
                return false;

            return TimeType == otherSettingConfig.TimeType
                && Source == otherSettingConfig.Source
                && MinuteAdjustment == otherSettingConfig.MinuteAdjustment
                && IsTimeShown == otherSettingConfig.IsTimeShown;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TimeType, Source, MinuteAdjustment, IsTimeShown);
        }
    }
}
