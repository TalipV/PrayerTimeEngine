﻿using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;

public class FaziletCityResponseDTO
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonPropertyName("adi")]
    public string Name { get; set; }
}
