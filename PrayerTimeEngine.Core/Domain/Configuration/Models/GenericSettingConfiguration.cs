using System.Text.Json.Serialization;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.ConfigStore.Models
{
    [JsonDerivedType(typeof(GenericSettingConfiguration), typeDiscriminator: "GenericConfig")]
    [JsonDerivedType(typeof(MuwaqqitDegreeCalculationConfiguration), typeDiscriminator: "MuwaqqitDegreeConfig")]
    public class GenericSettingConfiguration
    {
        public required ETimeType TimeType { get; init; }
        public virtual ECalculationSource Source { get; init; } = ECalculationSource.None;
        public int MinuteAdjustment { get; set; } = 0;
        public bool IsTimeShown { get; set; } = true;
    }
}
