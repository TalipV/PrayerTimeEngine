using System.Globalization;
using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services;

public class PlaceService(
        ILocationIQApiService locationIQApiService,
        ISystemInfoService systemInfoService,
        ILogger<PlaceService> logger,
        string apiKey
    ) : IPlaceService
{
    private readonly string _apiKey = apiKey;

    public async Task<ProfilePlaceInfo> GetTimezoneInfo(BasicPlaceInfo basicPlaceInfo, CancellationToken cancellationToken)
    {
        await EnsureCooldown(cancellationToken).ConfigureAwait(false);

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
        await EnsureCooldown(cancellationToken).ConfigureAwait(false);

        try
        {
            List<LocationIQPlace> places = await locationIQApiService.GetPlacesAsync(
                    language,
                    searchTerm,
                    _apiKey,
                    cancellationToken).ConfigureAwait(false);

            return [.. places.Select(x => GetlocationIQPlace(x, language)).Where(x => !string.IsNullOrWhiteSpace(x.City) && !string.IsNullOrWhiteSpace(x.Country))];
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public async Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo inputPlace, string language, CancellationToken cancellationToken)
    {
        await EnsureCooldown(cancellationToken).ConfigureAwait(false);

        LocationIQPlace place = await locationIQApiService.GetSpecificPlaceAsync(
                language,
                inputPlace.Latitude,
                inputPlace.Longitude,
                inputPlace.ExternalID,
                _apiKey,
                cancellationToken).ConfigureAwait(false);

        return GetlocationIQPlace(place, language);
    }

    // only allowed two calls per second
    private const int NECESSARY_COOL_DOWN_MS = 500;
    private static readonly AsyncNonKeyedLocker s_semaphore = new(1);
    private static Instant? s_lastCooldownCheck;

    private async Task EnsureCooldown(CancellationToken cancellationToken)
    {
        using (await s_semaphore.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                if (s_lastCooldownCheck is not null)
                {
                    Instant currentInstant = systemInfoService.GetCurrentInstant();
                    int millisecondsSinceLastCooldownCheck = (int)Math.Floor((currentInstant - s_lastCooldownCheck.Value).TotalMilliseconds);

                    if (millisecondsSinceLastCooldownCheck < NECESSARY_COOL_DOWN_MS)
                    {
                        await Task.Delay(NECESSARY_COOL_DOWN_MS - millisecondsSinceLastCooldownCheck, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception during cooldown logic");
                await Task.Delay(NECESSARY_COOL_DOWN_MS, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                s_lastCooldownCheck = systemInfoService.GetCurrentInstant();
                logger.LogDebug("Cooldown end at {Instant} ms", s_lastCooldownCheck.Value.ToUnixTimeMilliseconds());
            }
        }
    }

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
            CityDistrict = locationIQPlace.Address.Suburb,
            PostCode = locationIQPlace.Address.Postcode,
            Street = $"{locationIQPlace.Address.Road} {locationIQPlace.Address.HouseNumber}"
        };
    }
}
