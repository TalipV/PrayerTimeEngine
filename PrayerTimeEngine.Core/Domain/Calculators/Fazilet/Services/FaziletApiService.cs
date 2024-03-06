using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletApiService(
            HttpClient httpClient
        ) : IFaziletApiService
    {
        internal const string GET_COUNTRIES_URL = "daily?districtId=232&lang=1";

        public async Task<Dictionary<string, int>> GetCountries(CancellationToken cancellationToken)
        {
            Dictionary<string, int> countries = [];

            HttpResponseMessage response = await httpClient.GetAsync(GET_COUNTRIES_URL, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonElement mainJson = (await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)).RootElement;

            foreach (JsonElement countryJson in mainJson.GetProperty("ulkeler").EnumerateArray())
            {
                string countryName = countryJson.GetProperty("adi").GetString();
                int countryId = countryJson.GetProperty("id").GetInt32();
                countries.Add(countryName, countryId);
            }

            return countries;
        }

        internal const string GET_CITIES_BY_COUNTRY_URL = "cities-by-country?districtId=";

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID, CancellationToken cancellationToken)
        {
            Dictionary<string, int> cities = [];

            HttpResponseMessage response = await httpClient.GetAsync(GET_CITIES_BY_COUNTRY_URL + countryID, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonElement mainJson = (await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)).RootElement;

            foreach (JsonElement cityJson in mainJson.EnumerateArray())
            {
                string cityName = cityJson.GetProperty("adi").GetString();
                int cityId = cityJson.GetProperty("id").GetInt32();
                cities.Add(cityName, cityId);
            }

            return cities;
        }

        internal const string GET_TIMES_BY_CITY_URL = "daily?districtId={0}&lang=2";

        public async Task<List<FaziletPrayerTimes>> GetTimesByCityID(int cityID, CancellationToken cancellationToken)
        {
            // error case
            // response.StatusCode = ServiceUnavailable (503)
            // response.ReasonPhrase = "Service Unavailable"
            // response.Content.ReadAsStringAsync() = "{"message":"Sistemlerimizde bakım çalışması yaptığımız için geçici olarak hizmet veremiyoruz. Anlayışınız için teşekkür ederiz."}"

            List<FaziletPrayerTimes> prayerTimesList = [];

            string url = string.Format(GET_TIMES_BY_CITY_URL, cityID);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonElement mainJson = (await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)).RootElement;

            string timeZoneName = mainJson.GetProperty("bolge_saatdilimi").GetString();
            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timeZoneName];

            foreach (JsonElement timesJson in mainJson.GetProperty("vakitler").EnumerateArray())
            {
                prayerTimesList.Add(
                    new FaziletPrayerTimes
                    {
                        CityID = cityID,
                        Imsak = getZonedDateTime(timeZone, timesJson.GetProperty("imsak").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Fajr = getZonedDateTime(timeZone, timesJson.GetProperty("sabah").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Shuruq = getZonedDateTime(timeZone, timesJson.GetProperty("gunes").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Dhuhr = getZonedDateTime(timeZone, timesJson.GetProperty("ogle").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Asr = getZonedDateTime(timeZone, timesJson.GetProperty("ikindi").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Maghrib = getZonedDateTime(timeZone, timesJson.GetProperty("aksam").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Isha = getZonedDateTime(timeZone, timesJson.GetProperty("yatsi").EnumerateArray().First().GetProperty("tarih").GetString()),
                        Date = getZonedDateTime(timeZone, timesJson.GetProperty("ogle").EnumerateArray().First().GetProperty("tarih").GetString()).Date
                    }
                );
            }

            return prayerTimesList;
        }

        private static ZonedDateTime getZonedDateTime(DateTimeZone timeZone, string timeStr)
        {
            Instant instant = InstantPattern.ExtendedIso.Parse(timeStr).Value;
            return instant.InZone(timeZone);
        }
    }
}
