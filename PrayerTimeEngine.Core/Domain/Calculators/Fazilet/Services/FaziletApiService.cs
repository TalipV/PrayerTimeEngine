using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.DTOs;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletApiService(
            HttpClient httpClient
        ) : IFaziletApiService
    {
        // https://fazilettakvimi.com/api/cms/monthly-times?districtId=3478
        // Nur für Ramadan?

        internal const string GET_COUNTRIES_URL = "daily?districtId=232&lang=1";

        public async Task<FaziletGetCountriesResponseDTO> GetCountries(CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(GET_COUNTRIES_URL, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<FaziletGetCountriesResponseDTO>(jsonStream);
        }

        internal const string GET_CITIES_BY_COUNTRY_URL = "cities-by-country?districtId=";

        public async Task<List<FaziletCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(GET_CITIES_BY_COUNTRY_URL + countryID, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<FaziletCityResponseDTO>>(jsonStream);
        }

        internal const string GET_TIMES_BY_CITY_URL = "daily?districtId={0}&lang=2";

        public async Task<FaziletGetTimesByCityIDResponseDTO> GetTimesByCityID(int cityID, CancellationToken cancellationToken)
        {
            // error case
            // response.StatusCode = ServiceUnavailable (503)
            // response.ReasonPhrase = "Service Unavailable"
            // response.Content.ReadAsStringAsync() = "{"message":"Sistemlerimizde bakım çalışması yaptığımız için geçici olarak hizmet veremiyoruz. Anlayışınız için teşekkür ederiz."}"

            string url = string.Format(GET_TIMES_BY_CITY_URL, cityID);
            using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<FaziletGetTimesByCityIDResponseDTO>(jsonStream);
        }

    }
}
