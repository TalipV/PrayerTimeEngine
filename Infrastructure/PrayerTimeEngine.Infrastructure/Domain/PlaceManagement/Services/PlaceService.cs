using System.Globalization;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services;

/// <summary>
/// Place search and look ups through LocationIQ. Its rate limit (2 requests per second on the free plan)
/// is not handled here but by the HTTP pipeline of the client (see <c>ApiThrottle</c>): it spaces the
/// requests, waits for cooldowns after a 429 and retries.
/// </summary>
public class PlaceService(
        ILocationIQApiService locationIQApiService,
        string apiKey
    ) : IPlaceService
{
    private readonly string _apiKey = apiKey;

    public async Task<ProfilePlaceInfo> GetTimezoneInfo(BasicPlaceInfo basicPlaceInfo, CancellationToken cancellationToken)
    {
        LocationIQTimezoneResponseDTO locationIQTimezone =
            await locationIQApiService.GetTimezoneAsync(
                basicPlaceInfo.Latitude,
                basicPlaceInfo.Longitude,
                _apiKey,
                cancellationToken).ConfigureAwait(false);

        return new ProfilePlaceInfo()
        {
            ExternalID = basicPlaceInfo.ExternalID,
            Longitude = basicPlaceInfo.Longitude,
            Latitude = basicPlaceInfo.Latitude,
            InfoLanguageCode = basicPlaceInfo.InfoLanguageCode,
            Country = basicPlaceInfo.Country,
            City = basicPlaceInfo.City,
            CityDistrict = basicPlaceInfo.CityDistrict,
            PostCode = basicPlaceInfo.PostCode,
            Street = basicPlaceInfo.Street,
            TimezoneInfo = new TimezoneInfo
            {
                DisplayName = locationIQTimezone.LocationIQTimezone.ShortName,
                Name = locationIQTimezone.LocationIQTimezone.Name,
                UtcOffsetSeconds = locationIQTimezone.LocationIQTimezone.OffsetSeconds
            }
        };
    }

    public async Task<List<BasicPlaceInfo>> SearchPlacesAsync(string searchTerm, string language, CancellationToken cancellationToken)
    {
        try
        {
            List<LocationIQPlace> places = await locationIQApiService.GetPlacesAsync(
                    language,
                    searchTerm,
                    _apiKey,
                    cancellationToken).ConfigureAwait(false);

            return [.. places.Select(x => GetlocationIQPlace(x, language)).Where(x => !string.IsNullOrWhiteSpace(x.City) && !string.IsNullOrWhiteSpace(x.Country))];
        }
        catch (ExternalServiceException ex) when (ex.Failure == EExternalServiceFailure.NotFound)
        {
            return [];
        }
    }

    public async Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo inputPlace, string language, CancellationToken cancellationToken)
    {
        LocationIQPlace place = await locationIQApiService.GetSpecificPlaceAsync(
                language,
                inputPlace.Latitude,
                inputPlace.Longitude,
                inputPlace.ExternalID,
                _apiKey,
                cancellationToken).ConfigureAwait(false);

        return GetlocationIQPlace(place, language);
    }

    /// <summary>
    /// LocationIQ leaves out every address part it does not have, e.g. the district, post code and
    /// street when a whole city ("Wiesbaden") is selected. Those parts are stored as empty texts because
    /// the database columns do not allow NULL. Country and city stay as they are: places without them are
    /// filtered out of the search, and the providers fall back from a missing city to the state.
    /// </summary>
    private static BasicPlaceInfo GetlocationIQPlace(LocationIQPlace locationIQPlace, string languageCode)
    {
        return new BasicPlaceInfo
        {
            ExternalID = locationIQPlace.OsmID,
            Longitude = decimal.Parse(locationIQPlace.Longitude, CultureInfo.InvariantCulture),
            Latitude = decimal.Parse(locationIQPlace.Latitude, CultureInfo.InvariantCulture),
            InfoLanguageCode = languageCode,
            Country = locationIQPlace.Address.Country,
            State = locationIQPlace.Address.State,
            City = locationIQPlace.Address.City,
            CityDistrict = locationIQPlace.Address.Suburb ?? locationIQPlace.Address.Neighbourhood ?? string.Empty,
            PostCode = locationIQPlace.Address.Postcode ?? string.Empty,
            Street = $"{locationIQPlace.Address.Road} {locationIQPlace.Address.HouseNumber}".Trim()
        };
    }
}
