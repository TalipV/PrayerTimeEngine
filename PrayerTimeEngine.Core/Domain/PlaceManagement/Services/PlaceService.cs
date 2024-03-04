using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services
{
    public class PlaceService(
            HttpClient httpClient,
            ILogger<PlaceService> logger
        ) : IPlaceService
    {
        private const string ACCESS_TOKEN = "pk.c515e4d0e2c522f5b06068b45574bb68";
        private const string BASE_URL = @"https://eu1.locationiq.com/v1/";

        private Instant? lastCooldownCheck;

        private const int MAX_RESULTS = 10;
        private const string BASE_URL_TIMEZONE = $"https://eu1.locationiq.com/v1/timezone?key={ACCESS_TOKEN}&lat=47.2803835&lon=11.41337";

        public async Task<CompletePlaceInfo> GetTimezoneInfo(BasicPlaceInfo basicPlaceInfo, CancellationToken cancellationToken)
        {
            string url =
                $"{BASE_URL_TIMEZONE}timezone" +
                    $"?format=json" +
                    $"&key={ACCESS_TOKEN}" +
                    $"&lat={basicPlaceInfo.Latitude.ToString(CultureInfo.InvariantCulture)}" +
                    $"&lon={basicPlaceInfo.Longitude.ToString(CultureInfo.InvariantCulture)}";

            await ensureCooldown(cancellationToken).ConfigureAwait(false);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string jsonResult = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonResult).GetProperty("timezone");
            LocationIQTimezone locationIQTimezone = jsonElement.Deserialize<LocationIQTimezone>();

            return new CompletePlaceInfo(basicPlaceInfo)
            {
                TimezoneInfo = new TimezoneInfo
                {
                    DisplayName = locationIQTimezone.short_name,
                    Name = locationIQTimezone.name,
                    UtcOffsetSeconds = locationIQTimezone.offset_sec
                }
            };
        }

        public async Task<List<BasicPlaceInfo>> SearchPlacesAsync(string searchTerm, string language, CancellationToken cancellationToken)
        {
            string url =
                $"{BASE_URL}autocomplete" +
                    $"?format=json" +
                    $"&key={ACCESS_TOKEN}" +
                    $"&addressdetails=1" +
                    $"&limit={MAX_RESULTS}" +
                    $"&accept-language={language}" +
                    //$"&countrycodes={countryCodes}" +
                    $"&q={searchTerm}";

            await ensureCooldown(cancellationToken).ConfigureAwait(false);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return [];

            response.EnsureSuccessStatusCode();
            string jsonResult = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            return
                JsonSerializer.Deserialize<List<LocationIQPlace>>(jsonResult)
                    .Select(x => getlocationIQPlace(x, language))
                    .Where(x => !string.IsNullOrWhiteSpace(x.City) && !string.IsNullOrWhiteSpace(x.Country))
                    .ToList();
        }

        public async Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo place, string languageIdentif, CancellationToken cancellationToken)
        {
            string url =
                $"{BASE_URL}reverse" +
                $"?key={ACCESS_TOKEN}" +
                $"&format=json" +
                $"&accept-language={languageIdentif}" +
                $"&lat={place.Latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&lon={place.Longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&osm_id={place.ID}";

            await ensureCooldown(cancellationToken).ConfigureAwait(false);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string jsonResult = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return getlocationIQPlace(JsonSerializer.Deserialize<LocationIQPlace>(jsonResult), languageIdentif);
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

        private BasicPlaceInfo getlocationIQPlace(LocationIQPlace locationIQPlace, string languageCode)
        {
            return new BasicPlaceInfo(
                id: locationIQPlace.osm_id,
                longitude: decimal.Parse(locationIQPlace.lon, CultureInfo.InvariantCulture),
                latitude: decimal.Parse(locationIQPlace.lat, CultureInfo.InvariantCulture),
                infoLanguageCode: languageCode,
                country: locationIQPlace.address.country,
                city: locationIQPlace.address.city,
                cityDistrict: locationIQPlace.address.suburb,
                postCode: locationIQPlace.address.postcode,
                street: $"{locationIQPlace.address.road} {locationIQPlace.address.house_number}");
        }
    }
}
