using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimeProviders.Providers.MyMosq.Models.DTOs;

public class MyMosqResponseDTO
{
    [JsonPropertyName("prayerTimes")]
    public List<MyMosqPrayerTimesDTO> PrayerTimes { get; set; }
}