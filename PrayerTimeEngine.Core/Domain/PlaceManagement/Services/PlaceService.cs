using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.DTOs;
using System.Globalization;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services
{
    public class PlaceService(
            ILocationIQApiService locationIQApiService,
            ILogger<PlaceService> logger
        ) : IPlaceService
    {

        private Instant? lastCooldownCheck;

        public async Task<CompletePlaceInfo> GetTimezoneInfo(BasicPlaceInfo basicPlaceInfo, CancellationToken cancellationToken)
        {
            await ensureCooldown(cancellationToken).ConfigureAwait(false);

            LocationIQTimezoneResponseDTO locationIQTimezone = 
                await locationIQApiService.GetTimezoneAsync(
                    basicPlaceInfo.Latitude, 
                    basicPlaceInfo.Longitude, 
                    cancellationToken).ConfigureAwait(false);

            return new CompletePlaceInfo(basicPlaceInfo)
            {
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
            
            var places = await locationIQApiService.GetPlacesAsync(
                    language,
                    searchTerm,
                    cancellationToken).ConfigureAwait(false);

            return places.Select(x => getlocationIQPlace(x, language))
                .Where(x => !string.IsNullOrWhiteSpace(x.City) && !string.IsNullOrWhiteSpace(x.Country))
                .ToList();
        }

        public async Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo inputPlace, string language, CancellationToken cancellationToken)
        {
            var place = await locationIQApiService.GetSpecificPlaceAsync(
                    language,
                    inputPlace.Latitude,
                    inputPlace.Longitude,
                    inputPlace.ID,
                    cancellationToken).ConfigureAwait(false);

            return getlocationIQPlace(place, language);
        }

        // only allowed two calls per second
        private const int NECESSARY_COOL_DOWN_MS = 500;
        private readonly AsyncNonKeyedLocker _semaphore = new(1);

        private async Task ensureCooldown(CancellationToken cancellationToken)
        {
            using (await _semaphore.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (lastCooldownCheck != null)
                    {
                        Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
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
                    lastCooldownCheck = SystemClock.Instance.GetCurrentInstant();
                    logger.LogDebug("Cooldown end at {Instant} ms", lastCooldownCheck.Value.ToUnixTimeMilliseconds());
                }
            }
        }

        private static BasicPlaceInfo getlocationIQPlace(LocationIQPlace locationIQPlace, string languageCode)
        {
            return new BasicPlaceInfo(
                id: locationIQPlace.OsmID,
                longitude: decimal.Parse(locationIQPlace.Longitude, CultureInfo.InvariantCulture),
                latitude: decimal.Parse(locationIQPlace.Latitude, CultureInfo.InvariantCulture),
                infoLanguageCode: languageCode,
                country: locationIQPlace.Address.Country,
                city: locationIQPlace.Address.City,
                cityDistrict: locationIQPlace.Address.Suburb,
                postCode: locationIQPlace.Address.Postcode,
                street: $"{locationIQPlace.Address.Road} {locationIQPlace.Address.HouseNumber}");
        }
    }
}
