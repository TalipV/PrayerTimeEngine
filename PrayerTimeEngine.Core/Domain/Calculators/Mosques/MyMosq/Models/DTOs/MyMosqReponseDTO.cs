using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Models.DTOs;

public class MyMosqResponseDTO
{
    [JsonPropertyName("prayerTimes")]
    public List<MyMosqPrayerTimesDTO> PrayerTimes { get; set; }
}