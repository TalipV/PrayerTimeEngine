using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Models;

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

        // UNUSED!!
        private const int EXTENT_OF_DAYS_RETRIEVED = 5;

        public async Task<List<SemerkandPrayerTimes>> GetTimesByCityID(LocalDate date, int cityID)
        {
            // TODO: get timezone from place API
            DateTimeZone timezone = DateTimeZoneProviders.Tzdb[PrayerTimesConfigurationStorage.TIMEZONE];

            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            HttpResponseMessage response = await _httpClient.GetAsync(prayerTimesURL);
            response.EnsureSuccessStatusCode();
            string jsonPrayerTimesString = await response.Content.ReadAsStringAsync();

            // API adds "*" in front of 'Isha and Fajr to indicate some kind of special calculation,
            // which leads to problems with the automatic parsing of the string to the DateTime
            // e.g. "*23:54" instead of "23:54"
            jsonPrayerTimesString = jsonPrayerTimesString.Replace("*", "");

            JArray prayerTimesJArray = JArray.Parse(jsonPrayerTimesString);

            List<SemerkandPrayerTimes> allPrayerTimes = new();

            foreach (JObject prayerTimeJObject in prayerTimesJArray)
            {
                int currentDayOfYear = (int)prayerTimeJObject["DayOfYear"];
                LocalDate currentDate = new LocalDate(date.Year, 1, 1).PlusDays(currentDayOfYear - 1);

                SemerkandPrayerTimes prayerTime = new()
                {
                    DayOfYear = currentDayOfYear,
                    CityID = cityID,
                    Date = currentDate,

                    Fajr = getZonedDateTime(timezone, currentDate, (string)prayerTimeJObject["Fajr"]),
                    Shuruq = getZonedDateTime(timezone, currentDate, (string)prayerTimeJObject["Tulu"]),
                    Dhuhr = getZonedDateTime(timezone, currentDate, (string)prayerTimeJObject["Zuhr"]),
                    Asr = getZonedDateTime(timezone, currentDate, (string)prayerTimeJObject["Asr"]),
                    Maghrib = getZonedDateTime(timezone, currentDate, (string)prayerTimeJObject["Maghrib"]),
                    Isha = getZonedDateTime(timezone, currentDate, (string)prayerTimeJObject["Isha"])
                };

                allPrayerTimes.Add(prayerTime);
            }

            return allPrayerTimes;
        }
        
        private ZonedDateTime getZonedDateTime(DateTimeZone timezone, LocalDate date, string timeString)
        {
            LocalTime time =
                LocalTimePattern.CreateWithInvariantCulture("HH:mm")
                .Parse(timeString)
                .Value;

            // InZoneStrictly throws an exception if the time is inacceptable,
            // like within the skipped hour of DST or ambiguous duplicate hour
            return (date + time).InZoneStrictly(timezone);
        }
    }
}
