using MethodTimer;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletApiService : IFaziletApiService
    {
        private readonly HttpClient _httpClient;

        public FaziletApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        internal const string GET_COUNTRIES_URL = "daily?districtId=232&lang=1";

        public async Task<Dictionary<string, int>> GetCountries()
        {
            Dictionary<string, int> countries = new();

            HttpResponseMessage response = await _httpClient.GetAsync(GET_COUNTRIES_URL).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject jObject = JObject.Parse(json);

            foreach (JObject country in (JArray)jObject["ulkeler"])
            {
                string countryName = (string)country["adi"];
                int countryId = (int)country["id"];
                countries.Add(countryName, countryId);
            }

            return countries;
        }

        internal const string GET_CITIES_BY_COUNTRY_URL = "cities-by-country?districtId=";

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID)
        {
            Dictionary<string, int> cities = new Dictionary<string, int>();

            HttpResponseMessage response = await _httpClient.GetAsync(GET_CITIES_BY_COUNTRY_URL + countryID).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            foreach (JObject city in JArray.Parse(json))
            {
                string cityName = (string)city["adi"];
                int cityId = (int)city["id"];
                cities.Add(cityName, cityId);
            }

            return cities;
        }

        internal const string GET_TIMES_BY_CITY_URL = "daily?districtId={0}&lang=2";

        [Time]
        public async Task<List<FaziletPrayerTimes>> GetTimesByCityID(int cityID)
        {
            List<FaziletPrayerTimes> prayerTimesList = new List<FaziletPrayerTimes>();

            string url = string.Format(GET_TIMES_BY_CITY_URL, cityID);
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject jObject = JObject.Parse(json);
            JArray timesJArray = (JArray)jObject["vakitler"];

            string timeZoneName = jObject.GetValue("bolge_saatdilimi").Value<string>();
            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timeZoneName];

            foreach (JObject timesJObject in timesJArray)
            {
                prayerTimesList.Add(
                    new FaziletPrayerTimes
                    {
                        CityID = cityID,
                        Imsak = getZonedDateTime(timeZone, (string)timesJObject["imsak"][0]["tarih"]),
                        Fajr = getZonedDateTime(timeZone, (string)timesJObject["sabah"][0]["tarih"]),
                        Shuruq = getZonedDateTime(timeZone, (string)timesJObject["gunes"][0]["tarih"]),
                        Dhuhr = getZonedDateTime(timeZone, (string)timesJObject["ogle"][0]["tarih"]),
                        Asr = getZonedDateTime(timeZone, (string)timesJObject["ikindi"][0]["tarih"]),
                        Maghrib = getZonedDateTime(timeZone, (string)timesJObject["aksam"][0]["tarih"]),
                        Isha = getZonedDateTime(timeZone, (string)timesJObject["yatsi"][0]["tarih"]),
                        Date = getZonedDateTime(timeZone, (string)timesJObject["ogle"][0]["tarih"]).Date
                    }
                );
            }

            return prayerTimesList;
        }

        private ZonedDateTime getZonedDateTime(DateTimeZone timeZone, string timeStr)
        {
            Instant instant = 
                InstantPattern.CreateWithInvariantCulture("MM/dd/yyyy HH:mm:ss")
                .Parse(timeStr)
                .Value;

            return instant.InZone(timeZone);
        }
    }
}
