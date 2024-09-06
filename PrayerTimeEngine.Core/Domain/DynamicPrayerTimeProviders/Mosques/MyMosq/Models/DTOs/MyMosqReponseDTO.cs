using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.MyMosq.Models.DTOs;

public class MyMosqResponseDTO
{
    [JsonPropertyName("prayerTimes")]
    public List<MyMosqPrayerTimesDTO> PrayerTimes { get; set; }
}