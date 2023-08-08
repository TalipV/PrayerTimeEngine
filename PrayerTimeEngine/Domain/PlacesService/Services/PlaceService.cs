using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PrayerTimeEngine.Domain.LocationService.Models;
using PrayerTimeEngine.Domain.NominatimLocation.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PrayerTimeEngine.Domain.NominatimLocation.Interfaces
{
    public class PlaceService : IPlaceService
    {
        private const string ACCESS_TOKEN = "pk.48863ca2d711d3a0ec7b118d88a24623";
        private const string BASE_URL = @"https://eu1.locationiq.com/v1/";

        private DateTime? lastAPICall;

        private const int MAX_RESULTS = 4;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PlaceService> _logger;

        public PlaceService(HttpClient httpClient, ILogger<PlaceService> logger)
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
            lastAPICall = DateTime.Now;
            
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

            _logger.LogDebug("LOG PLACE SEARCH: {url}", url);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            lastAPICall = DateTime.Now;

            string jsonResult = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LocationIQPlace>(jsonResult);
        }

        private const double NECESSARY_COOL_DOWN_MS = 2000;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private async Task ensureCooldown()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (lastAPICall.HasValue)
                {
                    int milliseconds = (int)Math.Ceiling((DateTime.Now - lastAPICall.Value).TotalMilliseconds);

                    if (0 <= milliseconds && milliseconds <= NECESSARY_COOL_DOWN_MS)
                    {
                        await Task.Delay(milliseconds);
                    }

                    lastAPICall = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

    }
}
