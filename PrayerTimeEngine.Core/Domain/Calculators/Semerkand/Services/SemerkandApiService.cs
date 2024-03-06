using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandApiService(
            HttpClient httpClient
        ) : ISemerkandApiService
    {
        internal const string GET_COUNTRIES_URL = "http://semerkandtakvimi.semerkandmobile.com/countries?language=tr";

        public async Task<Dictionary<string, int>> GetCountries(CancellationToken cancellationToken)
        {
            Dictionary<string, int> countriesByCountryID = [];

            HttpResponseMessage response = await httpClient.GetAsync(GET_COUNTRIES_URL, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonElement mainJson = (await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)).RootElement;

            foreach (JsonElement item in mainJson.EnumerateArray())
            {
                int id = item.GetProperty("Id").GetInt32();
                string name = item.GetProperty("Name").GetString();

                countriesByCountryID[name] = id;
            }

            return countriesByCountryID;
        }

        internal const string GET_CITIES_BY_COUNTRY_URL = "https://www.semerkandtakvimi.com/Home/CityList";

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken)
        {
            Dictionary<string, int> citiesByCountryID = [];

            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("id", countryID.ToString())
            ]);

            HttpResponseMessage response = await httpClient.PostAsync(GET_CITIES_BY_COUNTRY_URL, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            
            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonElement mainJson = (await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)).RootElement;

            foreach (JsonElement item in mainJson.EnumerateArray())
            {
                int id = item.GetProperty("Id").GetInt32();
                string name = item.GetProperty("Name").GetString();

                citiesByCountryID[name] = id;
            }

            return citiesByCountryID;
        }

        internal const string GET_TIMES_BY_CITY = @"http://semerkandtakvimi.semerkandmobile.com/salaattimes?cityId={0}&year={1}";

        internal const int EXTENT_OF_DAYS_RETRIEVED = 5;

        public async Task<List<SemerkandPrayerTimes>> GetTimesByCityID(LocalDate date, string timezoneName, int cityID, CancellationToken cancellationToken)
        {
            List<SemerkandPrayerTimes> prayerTimes = [];

            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[timezoneName];
            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            HttpResponseMessage response = await httpClient.GetAsync(prayerTimesURL, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string jsonPrayerTimesString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            // API adds "*" in front of 'Isha and Fajr to indicate some kind of special calculation,
            // which leads to problems with the automatic parsing of the string to the DateTime
            // e.g. "*23:54" instead of "23:54"
            jsonPrayerTimesString = jsonPrayerTimesString.Replace("*", "");

            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonElement mainJson = (await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)).RootElement;

            foreach (JsonElement prayerTimeJson in mainJson.EnumerateArray())
            {
                int currentDayOfYear = prayerTimeJson.GetProperty("DayOfYear").GetInt32();
                LocalDate currentDate = new LocalDate(date.Year, 1, 1).PlusDays(currentDayOfYear - 1);

                // ignore past and ignore what is after the necessary extent
                if (currentDate < date || date.PlusDays(EXTENT_OF_DAYS_RETRIEVED - 1) < currentDate)
                    continue;

                SemerkandPrayerTimes prayerTime = new()
                {
                    DayOfYear = currentDayOfYear,
                    CityID = cityID,
                    Date = currentDate,

                    Fajr = getZonedDateTime(dateTimeZone, currentDate, prayerTimeJson.GetProperty("Fajr").GetString()),
                    Shuruq = getZonedDateTime(dateTimeZone, currentDate, prayerTimeJson.GetProperty("Tulu").GetString()),
                    Dhuhr = getZonedDateTime(dateTimeZone, currentDate, prayerTimeJson.GetProperty("Zuhr").GetString()),
                    Asr = getZonedDateTime(dateTimeZone, currentDate, prayerTimeJson.GetProperty("Asr").GetString()),
                    Maghrib = getZonedDateTime(dateTimeZone, currentDate, prayerTimeJson.GetProperty("Maghrib").GetString()),
                    Isha = getZonedDateTime(dateTimeZone, currentDate, prayerTimeJson.GetProperty("Isha").GetString())
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
