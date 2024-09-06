using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Models
{
    [JsonDerivedType(typeof(MuwaqqitLocationData), typeDiscriminator: "MuwaqqitLocationData")]
    [JsonDerivedType(typeof(SemerkandLocationData), typeDiscriminator: "SemerkandLocationData")]
    [JsonDerivedType(typeof(FaziletLocationData), typeDiscriminator: "FaziletLocationData")]
    public abstract class BaseLocationData
    {
        [JsonIgnore]
        public abstract EDynamicPrayerTimeProviderType Source { get; }
    }
}
