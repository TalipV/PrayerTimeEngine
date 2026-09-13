using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;

public interface ILocationIQApiService
{
    private const string MAX_RESULTS = "10";

    [Get("/timezone?format=json")]
    Task<LocationIQTimezoneResponseDTO> GetTimezoneAsync(
        [AliasAs("lat")] decimal latitude,
        [AliasAs("lon")] decimal longitude,
        [AliasAs("key")] string accessToken,
        CancellationToken cancellationToken);

    // &countrycodes is also available to restrict results to countries
    [Get($"/autocomplete?format=json&addressdetails=1&limit={MAX_RESULTS}")]
    Task<List<LocationIQPlace>> GetPlacesAsync(
        [AliasAs("accept-language")] string language,
        [AliasAs("q")] string searchTerm,
        [AliasAs("key")] string accessToken,
        CancellationToken cancellationToken);

    [Get("/reverse?format=json")]
    Task<LocationIQPlace> GetSpecificPlaceAsync(
        [AliasAs("accept-language")] string language,
        [AliasAs("lat")] decimal latitude,
        [AliasAs("lon")] decimal longitude,
        [AliasAs("osm_id")] string osmID,
        [AliasAs("key")] string accessToken,
        CancellationToken cancellationToken);
}
