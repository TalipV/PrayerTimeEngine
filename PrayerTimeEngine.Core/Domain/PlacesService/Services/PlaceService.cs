using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngine.Core.Domain.PlacesService.Models;
using System.Net;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.PlacesService.Services
{
    public class LocationService : ILocationService
    {
        private const string ACCESS_TOKEN = "pk.48863ca2d711d3a0ec7b118d88a24623";
        private const string BASE_URL = @"https://eu1.locationiq.com/v1/";

        private Instant? lastAPICallInstant;

        private const int MAX_RESULTS = 4;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationService> _logger;

        public LocationService(HttpClient httpClient, ILogger<LocationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<LocationIQPlace>> SearchPlacesAsync(string searchTerm, string language)
        {
            await ensureCooldown();

            string countryCodes = WebUtility.UrlEncode("de,at");
            string url =
                $"{BASE_URL}search" +
                    $"?format=json" +
                    $"&key={ACCESS_TOKEN}" +
                    $"&addressdetails=1" +
                    $"&limit={MAX_RESULTS}" +
                    $"&accept-language={language}" +
                    $"&countrycodes={countryCodes}" +
                    $"&q={searchTerm}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            lastAPICallInstant = SystemClock.Instance.GetCurrentInstant();

            string jsonResult = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LocationIQPlace>>(jsonResult);
        }

        public async Task<LocationIQPlace> GetPlaceByID(LocationIQPlace place, string languageIdentif)
        {
            await ensureCooldown();

            string url =
                $"{BASE_URL}reverse" +
                $"?key={ACCESS_TOKEN}" +
                $"&format=json" +
                $"&accept-language={languageIdentif}" +
                $"&lat={place.lat}" +
                $"&lon={place.lon}" +
                $"&osm_id={place.osm_id}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            lastAPICallInstant = SystemClock.Instance.GetCurrentInstant();

            string jsonResult = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LocationIQPlace>(jsonResult);
        }

        private const double NECESSARY_COOL_DOWN_MS = 3000;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private async Task ensureCooldown()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (lastAPICallInstant != null)
                {
                    int milliseconds = (int)Math.Ceiling((SystemClock.Instance.GetCurrentInstant() - lastAPICallInstant.Value).TotalMilliseconds);

                    if (0 <= milliseconds && milliseconds <= NECESSARY_COOL_DOWN_MS)
                    {
                        await Task.Delay(milliseconds);
                    }

                    lastAPICallInstant = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

    }
}
