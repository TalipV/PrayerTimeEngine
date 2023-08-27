using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Domain.Model
{
    [JsonDerivedType(typeof(MuwaqqitLocationData), typeDiscriminator: "MuwaqqitLocationData")]
    [JsonDerivedType(typeof(SemerkandLocationData), typeDiscriminator: "SemerkandLocationData")]
    [JsonDerivedType(typeof(FaziletLocationData), typeDiscriminator: "FaziletLocationData")]
    public abstract class BaseLocationData
    {
        [JsonIgnore]
        public abstract ECalculationSource Source { get; }
    }
}
