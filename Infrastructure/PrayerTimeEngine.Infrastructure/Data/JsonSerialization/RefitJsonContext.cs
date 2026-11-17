using System.Text.Json;
using System.Text.Json.Serialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;

namespace PrayerTimeEngine.Core.Data.JsonSerialization;

/// <summary>
/// Source-generated JSON metadata for every type the Refit clients receive.
/// Refit 17 no longer falls back to reflection-based System.Text.Json, so a type that is
/// missing here fails at runtime when its response is read. Add new response DTOs here.
/// </summary>
/// <remarks>
/// <see cref="JsonSerializerDefaults.Web"/> matches the options Refit 16 used by default
/// (case-insensitive property names, numbers readable from strings).
/// </remarks>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
// Fazilet
[JsonSerializable(typeof(FaziletGetCountriesResponseDTO))]
[JsonSerializable(typeof(List<FaziletCityResponseDTO>))]
[JsonSerializable(typeof(FaziletGetTimesByCityIDResponseDTO))]
// Muwaqqit
[JsonSerializable(typeof(MuwaqqitPrayerTimesResponseDTO))]
// Semerkand
[JsonSerializable(typeof(List<SemerkandCountryResponseDTO>))]
[JsonSerializable(typeof(List<SemerkandCityResponseDTO>))]
[JsonSerializable(typeof(List<SemerkandPrayerTimesResponseDTO>))]
// LocationIQ
[JsonSerializable(typeof(LocationIQTimezoneResponseDTO))]
[JsonSerializable(typeof(List<LocationIQPlace>))]
[JsonSerializable(typeof(LocationIQPlace))]
internal sealed partial class RefitJsonContext : JsonSerializerContext;
