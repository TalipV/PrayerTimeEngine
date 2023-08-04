using PrayerTimeEngine.Domain.LocationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.NominatimLocation.Interfaces
{
    public class PlaceService : IPlaceService
    {
        private readonly HttpClient _httpClient;

        public PlaceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Place>> SearchPlacesAsync(string searchTerm, string language)
        {
            string url = $"search?format=json&addressdetails=1&q={searchTerm}";
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string jsonResult = await response.Content.ReadAsStringAsync();
                var places = JsonSerializer.Deserialize<List<Place>>(jsonResult);
                return places;
            }

            throw new NotImplementedException();
        }
    }
}
