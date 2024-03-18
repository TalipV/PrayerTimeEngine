using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandApiService(
            HttpClient httpClient
        ) : ISemerkandApiService
    {
        internal const string GET_COUNTRIES_URL = "http://semerkandtakvimi.semerkandmobile.com/countries?language=tr";

        public async Task<List<SemerkandCountryResponseDTO>> GetCountries(CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(GET_COUNTRIES_URL, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            return JsonSerializer.Deserialize<List<SemerkandCountryResponseDTO>>(jsonStream);
        }

        internal const string GET_CITIES_BY_COUNTRY_URL = "https://www.semerkandtakvimi.com/Home/CityList";

        public async Task<List<SemerkandCityResponseDTO>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("id", countryID.ToString())
            ]);

            using HttpResponseMessage response = await httpClient.PostAsync(GET_CITIES_BY_COUNTRY_URL, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            return JsonSerializer.Deserialize<List<SemerkandCityResponseDTO>>(jsonStream);
        }

        // returns the times for the specified time and the subsequent 30 days
        // https://www.semerkandtakvimi.com/Home/CityTimeList?City=32&Year=2024&Day=76

        internal const string GET_TIMES_BY_CITY = @"http://semerkandtakvimi.semerkandmobile.com/salaattimes?cityId={0}&year={1}";

        public async Task<List<SemerkandPrayerTimesResponseDTO>> GetTimesByCityID(LocalDate date, int cityID, CancellationToken cancellationToken)
        {
            List<SemerkandPrayerTimesResponseDTO> returnList = [];

            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            using HttpResponseMessage response = await httpClient.GetAsync(prayerTimesURL, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await foreach (SemerkandPrayerTimesResponseDTO prayerTimesDTO in JsonSerializer.DeserializeAsyncEnumerable<SemerkandPrayerTimesResponseDTO>(jsonStream, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                returnList.Add(prayerTimesDTO);
            }

            return returnList;
        }
    }
}
