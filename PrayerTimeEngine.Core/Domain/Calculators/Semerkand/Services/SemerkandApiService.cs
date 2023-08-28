using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using System.Globalization;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandApiService : ISemerkandApiService
    {
        private readonly HttpClient _httpClient;

        public SemerkandApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        internal const string GET_COUNTRIES_URL = "http://semerkandtakvimi.semerkandmobile.com/countries?language=tr";

        public async Task<Dictionary<string, int>> GetCountries()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(GET_COUNTRIES_URL);

            response.EnsureSuccessStatusCode();
            string jsonCitiesString = await response.Content.ReadAsStringAsync();

            Dictionary<string, int> countriesByCountryID = new Dictionary<string, int>();

            foreach (JToken item in JArray.Parse(jsonCitiesString))
            {
                var cityJSON = JObject.Parse(item.ToString());

                int id = cityJSON["Id"].Value<int>();
                string name = cityJSON["Name"].Value<string>();

                countriesByCountryID[name] = id;
            }

            return countriesByCountryID;
        }

        internal const string GET_CITIES_BY_COUNTRY_URL = "https://www.semerkandtakvimi.com/Home/CityList";

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", countryID.ToString())
            });

            HttpResponseMessage response = await _httpClient.PostAsync(GET_CITIES_BY_COUNTRY_URL, content);
            response.EnsureSuccessStatusCode();
            string jsonCitiesString = await response.Content.ReadAsStringAsync();

            Dictionary<string, int> citiesByCountryID = new Dictionary<string, int>();

            foreach (JToken item in JArray.Parse(jsonCitiesString))
            {
                var cityJSON = JObject.Parse(item.ToString());

                int id = cityJSON["Id"].Value<int>();
                string name = cityJSON["Name"].Value<string>();

                citiesByCountryID[name] = id;
            }

            return citiesByCountryID;
        }

        internal const string GET_TIMES_BY_CITY = @"http://semerkandtakvimi.semerkandmobile.com/salaattimes?cityId={0}&year={1}";

        private static readonly JsonSerializerSettings settings =
            new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Culture = CultureInfo.InvariantCulture
            };

        private const int EXTENT_OF_DAYS_RETRIEVED = 5;

        public async Task<List<SemerkandPrayerTimes>> GetTimesByCityID(DateTime date, int cityID)
        {
            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            HttpResponseMessage response = await _httpClient.GetAsync(prayerTimesURL);
            response.EnsureSuccessStatusCode();
            string jsonPrayerTimesString = await response.Content.ReadAsStringAsync();

            // API adds "*" in front of 'Isha and Fajr to indicate some kind of special calculation,
            // which leads to problems with the automatic parsing of the string to the DateTime
            // e.g. "*23:54" instead of "23:54"
            jsonPrayerTimesString = jsonPrayerTimesString.Replace("*", "");

            List<SemerkandPrayerTimes> allPrayerTimes = JsonConvert.DeserializeObject<List<SemerkandPrayerTimes>>(jsonPrayerTimesString, settings);

            foreach (SemerkandPrayerTimes prayerTime in allPrayerTimes)
            {
                DateTime currentPrayerTimeDate =
                    new DateTime(date.Year, 1, 1)
                        .AddDays(prayerTime.DayOfYear - 1);

                prayerTime.CityID = cityID;
                prayerTime.Date = currentPrayerTimeDate;
                prayerTime.Fajr = getFullDateTime(currentPrayerTimeDate, prayerTime.Fajr);
                prayerTime.Shuruq = getFullDateTime(currentPrayerTimeDate, prayerTime.Shuruq);
                prayerTime.Dhuhr = getFullDateTime(currentPrayerTimeDate, prayerTime.Dhuhr);
                prayerTime.Asr = getFullDateTime(currentPrayerTimeDate, prayerTime.Asr);
                prayerTime.Maghrib = getFullDateTime(currentPrayerTimeDate, prayerTime.Maghrib);
                prayerTime.Isha = getFullDateTime(currentPrayerTimeDate, prayerTime.Isha);
            }

            DateTime minDateTime = date.Date;
            DateTime maxDateTime = date.Date.AddDays(EXTENT_OF_DAYS_RETRIEVED);

            return allPrayerTimes.Where(pt => minDateTime <= pt.Date && pt.Date < maxDateTime).ToList();
        }

        private DateTime getFullDateTime(DateTime actualDate, DateTime onlyDayTime)
        {
            return new DateTime(
                actualDate.Year,
                actualDate.Month,
                actualDate.Day,
                onlyDayTime.Hour,
                onlyDayTime.Minute,
                onlyDayTime.Second);
        }
    }
}
