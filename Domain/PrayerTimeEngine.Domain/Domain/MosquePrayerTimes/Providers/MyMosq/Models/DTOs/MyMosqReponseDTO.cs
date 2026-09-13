using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.DTOs;

public class MyMosqResponseDTO
{
    [JsonPropertyName("prayerTimes")]
    public List<MyMosqPrayerTimesDTO> PrayerTimes { get; set; }
}