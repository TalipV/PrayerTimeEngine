using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandApiService(
            HttpClient httpClient
        ) : ISemerkandApiService
    {
        internal const string GET_COUNTRIES_URL = "http://semerkandtakvimi.semerkandmobile.com/countries?language=tr";

        public async Task<Dictionary<string, int>> GetCountries()
        {
            HttpResponseMessage response = await httpClient.GetAsync(GET_COUNTRIES_URL).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            string jsonCitiesString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Dictionary<string, int> countriesByCountryID = [];

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
            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("id", countryID.ToString())
            ]);

            HttpResponseMessage response = await httpClient.PostAsync(GET_CITIES_BY_COUNTRY_URL, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string jsonCitiesString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Dictionary<string, int> citiesByCountryID = [];

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

        internal const int EXTENT_OF_DAYS_RETRIEVED = 5;

        public async Task<List<SemerkandPrayerTimes>> GetTimesByCityID(LocalDate date, string timezoneName, int cityID)
        {
            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[timezoneName];
            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            HttpResponseMessage response = await httpClient.GetAsync(prayerTimesURL).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string jsonPrayerTimesString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // API adds "*" in front of 'Isha and Fajr to indicate some kind of special calculation,
            // which leads to problems with the automatic parsing of the string to the DateTime
            // e.g. "*23:54" instead of "23:54"
            jsonPrayerTimesString = jsonPrayerTimesString.Replace("*", "");

            JArray prayerTimesJArray = JArray.Parse(jsonPrayerTimesString);

            List<SemerkandPrayerTimes> prayerTimes = [];

            foreach (JObject prayerTimeJObject in prayerTimesJArray.Cast<JObject>())
            {
                int currentDayOfYear = (int)prayerTimeJObject["DayOfYear"];
                LocalDate currentDate = new LocalDate(date.Year, 1, 1).PlusDays(currentDayOfYear - 1);

                // ignore past and ignore what is after the necessary extent
                if (currentDate < date || date.PlusDays(EXTENT_OF_DAYS_RETRIEVED - 1) < currentDate)
                    continue;

                SemerkandPrayerTimes prayerTime = new()
                {
                    DayOfYear = currentDayOfYear,
                    CityID = cityID,
                    Date = currentDate,

                    Fajr = getZonedDateTime(dateTimeZone, currentDate, (string)prayerTimeJObject["Fajr"]),
                    Shuruq = getZonedDateTime(dateTimeZone, currentDate, (string)prayerTimeJObject["Tulu"]),
                    Dhuhr = getZonedDateTime(dateTimeZone, currentDate, (string)prayerTimeJObject["Zuhr"]),
                    Asr = getZonedDateTime(dateTimeZone, currentDate, (string)prayerTimeJObject["Asr"]),
                    Maghrib = getZonedDateTime(dateTimeZone, currentDate, (string)prayerTimeJObject["Maghrib"]),
                    Isha = getZonedDateTime(dateTimeZone, currentDate, (string)prayerTimeJObject["Isha"])
                };

                prayerTimes.Add(prayerTime);
            }

            return prayerTimes;
        }
        
        private static ZonedDateTime getZonedDateTime(DateTimeZone timezone, LocalDate date, string timeString)
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
