using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models
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
