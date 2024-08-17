using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ.DTOs;
using Refit;
using System.Globalization;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services
{
    public class PlaceService(
            ILocationIQApiService locationIQApiService,
            ISystemInfoService systemInfoService,
            ILogger<PlaceService> logger
        ) : IPlaceService
    {
        private const string _apiKey = "pk.5fdf59f9c3682abc89749e42d1f68d8d";

        public async Task<CompletePlaceInfo> GetTimezoneInfo(BasicPlaceInfo basicPlaceInfo, CancellationToken cancellationToken)
        {
            await ensureCooldown(cancellationToken).ConfigureAwait(false);

            LocationIQTimezoneResponseDTO locationIQTimezone =
                await locationIQApiService.GetTimezoneAsync(
                    basicPlaceInfo.Latitude,
                    basicPlaceInfo.Longitude,
                    _apiKey,
                    cancellationToken).ConfigureAwait(false);

            return new CompletePlaceInfo()
            {
                OrmID = basicPlaceInfo.OrmID,
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
            await ensureCooldown(cancellationToken).ConfigureAwait(false);

            try
            {
                var places = await locationIQApiService.GetPlacesAsync(
                        language,
                        searchTerm,
                        _apiKey,
                        cancellationToken).ConfigureAwait(false);

                return places.Select(x => getlocationIQPlace(x, language))
                    .Where(x => !string.IsNullOrWhiteSpace(x.City) && !string.IsNullOrWhiteSpace(x.Country))
                    .ToList();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return [];
            }
        }

        public async Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo inputPlace, string language, CancellationToken cancellationToken)
        {
            await ensureCooldown(cancellationToken).ConfigureAwait(false);

            var place = await locationIQApiService.GetSpecificPlaceAsync(
                    language,
                    inputPlace.Latitude,
                    inputPlace.Longitude,
                    inputPlace.OrmID,
                    _apiKey,
                    cancellationToken).ConfigureAwait(false);

            return getlocationIQPlace(place, language);
        }

        // only allowed two calls per second
        private const int NECESSARY_COOL_DOWN_MS = 500;
        private static readonly AsyncNonKeyedLocker _semaphore = new(1);
        private static Instant? lastCooldownCheck;

        private async Task ensureCooldown(CancellationToken cancellationToken)
        {
            using (await _semaphore.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (lastCooldownCheck != null)
                    {
                        Instant currentInstant = systemInfoService.GetCurrentInstant();
                        int millisecondsSinceLastCooldownCheck = (int)Math.Floor((currentInstant - lastCooldownCheck.Value).TotalMilliseconds);

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
                    lastCooldownCheck = systemInfoService.GetCurrentInstant();
                    logger.LogDebug("Cooldown end at {Instant} ms", lastCooldownCheck.Value.ToUnixTimeMilliseconds());
                }
            }
        }

        private static BasicPlaceInfo getlocationIQPlace(LocationIQPlace locationIQPlace, string languageCode)
        {
            return new BasicPlaceInfo
            {
                OrmID = locationIQPlace.OsmID,
                Longitude = decimal.Parse(locationIQPlace.Longitude, CultureInfo.InvariantCulture),
                Latitude = decimal.Parse(locationIQPlace.Latitude, CultureInfo.InvariantCulture),
                InfoLanguageCode = languageCode,
                Country = locationIQPlace.Address.Country,
                City = locationIQPlace.Address.City,
                CityDistrict = locationIQPlace.Address.Suburb,
                PostCode = locationIQPlace.Address.Postcode,
                Street = $"{locationIQPlace.Address.Road} {locationIQPlace.Address.HouseNumber}"
            };
        }
    }
}
