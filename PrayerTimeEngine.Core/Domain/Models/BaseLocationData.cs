using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Semerkand.Models;
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
