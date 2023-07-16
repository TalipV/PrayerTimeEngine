using Newtonsoft.Json.Linq;
using PrayerTimeEngine.Code.Domain.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using SQLitePCL;
using System.Globalization;

namespace PrayerTimeEngine.Code.Domain.Fazilet.Services
{
    public class FaziletApiService : IFaziletApiService
    {
        private const int API_TIMEOUT_SECONDS = 10;

        public FaziletApiService()
        {
        }

        private const string GET_COUNTRIES_URL = "https://fazilettakvimi.com/api/cms/daily?districtId=232&lang=0";

        public Dictionary<string, int> GetCountries()
        {
            Dictionary<string, int> countries = new Dictionary<string, int>();

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(API_TIMEOUT_SECONDS);

                HttpResponseMessage response = client.GetAsync(GET_COUNTRIES_URL).Result;
                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    JObject jObject = JObject.Parse(json);

                    foreach (JObject country in (JArray)jObject["ulkeler"])
                    {
                        string countryName = (string)country["adi"];
                        int countryId = (int)country["id"];
                        countries.Add(countryName, countryId);
                    }
                }
            }

            return countries;
        }

        private const string GET_CITIES_BY_COUNTRY_URL = "https://fazilettakvimi.com/api/cms/cities-by-country?districtId=";

        public Dictionary<string, int> GetCitiesByCountryID(int countryID)
        {
            Dictionary<string, int> cities = new Dictionary<string, int>();

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(API_TIMEOUT_SECONDS);

                HttpResponseMessage response = client.GetAsync(GET_CITIES_BY_COUNTRY_URL + countryID).Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;

                    foreach (JObject city in JArray.Parse(json))
                    {
                        string cityName = (string)city["adi"];
                        int cityId = (int)city["id"];
                        cities.Add(cityName, cityId);
                    }
                }
            }

            return cities;
        }

        private const string GET_TIMES_BY_CITY_URL = "https://fazilettakvimi.com/api/cms/daily?districtId={0}&lang=1";

        public List<FaziletPrayerTimes> GetTimesByCityID(int cityID)
        {
            List<FaziletPrayerTimes> prayerTimesList = new List<FaziletPrayerTimes>();

            string url = string.Format(GET_TIMES_BY_CITY_URL, cityID);

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(API_TIMEOUT_SECONDS);

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    JObject jObject = JObject.Parse(json);
                    JArray timesArray = (JArray)jObject["vakitler"];

                    string timeZoneName = jObject.GetValue("bolge_saatdilimi").Value<string>();
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);

                    foreach (JObject times in timesArray)
                    {
                        DateTime imsak = getTimeJSONAsDateTime(times, "imsak", timeZoneInfo);
                        DateTime fajr = getTimeJSONAsDateTime(times, "sabah", timeZoneInfo);
                        DateTime shuruq = getTimeJSONAsDateTime(times, "gunes", timeZoneInfo);
                        DateTime dhuhr = getTimeJSONAsDateTime(times, "ogle", timeZoneInfo);
                        DateTime asr = getTimeJSONAsDateTime(times, "ikindi", timeZoneInfo);
                        DateTime maghrib = getTimeJSONAsDateTime(times, "aksam", timeZoneInfo);
                        DateTime isha = getTimeJSONAsDateTime(times, "yatsi", timeZoneInfo);

                        FaziletPrayerTimes prayerTimes = new FaziletPrayerTimes(cityID, imsak, fajr, shuruq, dhuhr, asr, maghrib, isha);
                        prayerTimesList.Add(prayerTimes);
                    }
                }
            }

            return prayerTimesList;
        }

        private static DateTime getTimeJSONAsDateTime(JObject times, string timeName, TimeZoneInfo timeZoneInfo)
        {
            DateTime dateTime =
                DateTime.ParseExact(
                    (string)times[timeName][0]["tarih"],
                    "MM/dd/yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

            return TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
        }
    }
}
