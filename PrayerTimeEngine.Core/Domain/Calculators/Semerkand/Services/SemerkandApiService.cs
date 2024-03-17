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

            using HttpResponseMessage response = await httpClient.GetAsync(GET_COUNTRIES_URL, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await foreach (JsonElement item in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(jsonStream, cancellationToken: cancellationToken).ConfigureAwait(false))
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

            using HttpResponseMessage response = await httpClient.PostAsync(GET_CITIES_BY_COUNTRY_URL, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await foreach (JsonElement item in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(jsonStream, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                int id = item.GetProperty("Id").GetInt32();
                string name = item.GetProperty("Name").GetString();

                citiesByCountryID[name] = id;
            }

            return citiesByCountryID;
        }

        // returns the times for the specified time and the subsequent 30 days
        // https://www.semerkandtakvimi.com/Home/CityTimeList?City=32&Year=2024&Day=76

        internal const string GET_TIMES_BY_CITY = @"http://semerkandtakvimi.semerkandmobile.com/salaattimes?cityId={0}&year={1}";

        internal const int EXTENT_OF_DAYS_RETRIEVED = 5;

        public async Task<List<SemerkandPrayerTimes>> GetTimesByCityID(LocalDate date, string timezoneName, int cityID, CancellationToken cancellationToken)
        {
            List<SemerkandPrayerTimes> prayerTimes = [];

            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[timezoneName];
            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            using HttpResponseMessage response = await httpClient.GetAsync(prayerTimesURL, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var firstDayOfYear = new LocalDate(date.Year, 1, 1);

            await foreach (JsonElement prayerTimeJson in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(jsonStream, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                int dayOfYear = prayerTimeJson.GetProperty("DayOfYear").GetInt32();
                LocalDate prayerTimeDate = firstDayOfYear.PlusDays(dayOfYear - 1);

                // ignore past and ignore what is after the necessary extent
                if (prayerTimeDate < date || date.PlusDays(EXTENT_OF_DAYS_RETRIEVED - 1) < prayerTimeDate)
                    continue;

                SemerkandPrayerTimes prayerTime = new()
                {
                    DayOfYear = dayOfYear,
                    CityID = cityID,
                    Date = prayerTimeDate,

                    Fajr = getZonedDateTime(dateTimeZone, prayerTimeDate, prayerTimeJson.GetProperty("Fajr").GetString()),
                    Shuruq = getZonedDateTime(dateTimeZone, prayerTimeDate, prayerTimeJson.GetProperty("Tulu").GetString()),
                    Dhuhr = getZonedDateTime(dateTimeZone, prayerTimeDate, prayerTimeJson.GetProperty("Zuhr").GetString()),
                    Asr = getZonedDateTime(dateTimeZone, prayerTimeDate, prayerTimeJson.GetProperty("Asr").GetString()),
                    Maghrib = getZonedDateTime(dateTimeZone, prayerTimeDate, prayerTimeJson.GetProperty("Maghrib").GetString()),
                    Isha = getZonedDateTime(dateTimeZone, prayerTimeDate, prayerTimeJson.GetProperty("Isha").GetString())
                };

                prayerTimes.Add(prayerTime);
            }

            return prayerTimes;
        }
        
        private static ZonedDateTime getZonedDateTime(DateTimeZone timezone, LocalDate date, string timeString)
        {
            // API adds "*" in front of 'Isha and Fajr to indicate some kind of special calculation,
            // which leads to problems with the automatic parsing of the string to the DateTime
            // e.g. "*23:54" instead of "23:54"

            LocalTime time =
                LocalTimePattern.CreateWithInvariantCulture("HH:mm")
                .Parse(timeString.Replace("*", ""))
                .Value;

            // InZoneStrictly throws an exception if the time is inacceptable,
            // like within the skipped hour of DST or ambiguous duplicate hour
            return (date + time).InZoneStrictly(timezone);
        }
    }
}
