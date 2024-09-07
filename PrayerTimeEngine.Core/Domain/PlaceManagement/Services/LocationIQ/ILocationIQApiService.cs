using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;

public interface ILocationIQApiService
{
    private const string MAX_RESULTS = "10";

    [Get($$"""/timezone?format=json&key={accessToken}&lat={latitude}&lon={longitude}""")]
    Task<LocationIQTimezoneResponseDTO> GetTimezoneAsync(
        decimal latitude,
        decimal longitude,
        string accessToken,
        CancellationToken cancellationToken);

    // &countrycodes is also available to restrict results to countries
    [Get($$"""/autocomplete?format=json&key={accessToken}&addressdetails=1&limit={{MAX_RESULTS}}&accept-language={language}&q={searchTerm}""")]
    Task<List<LocationIQPlace>> GetPlacesAsync(
        string language,
        string searchTerm,
        string accessToken,
        CancellationToken cancellationToken);

    [Get($$"""/reverse?format=json&key={accessToken}&accept-language={language}&lat={latitude}&lon={longitude}&osm_id={osmID}""")]
    Task<LocationIQPlace> GetSpecificPlaceAsync(
        string language,
        decimal latitude,
        decimal longitude,
        string osmID,
        string accessToken,
        CancellationToken cancellationToken);
}
