﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Interfaces;
using System.Globalization;
using System.Web;

namespace PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Services
{
    public class SemerkandApiService : ISemerkandApiService
    {
        private readonly HttpClient _httpClient;

        public SemerkandApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private const string GET_COUNTRIES_URL = "http://semerkandtakvimi.semerkandmobile.com/countries?language=tr";

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

        private const string GET_CITIES_BY_COUNTRY_URL = "https://www.semerkandtakvimi.com/Home/CityList";

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

        private const string GET_TIMES_BY_CITY = @"http://semerkandtakvimi.semerkandmobile.com/salaattimes?cityId={0}&year={1}";

        private static readonly JsonSerializerSettings settings = 
            new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Culture = CultureInfo.InvariantCulture
            };

        public async Task<List<SemerkandPrayerTimes>> GetTimesByCityID(DateTime date, int cityID)
        {
            string prayerTimesURL = string.Format(GET_TIMES_BY_CITY, cityID, date.Year);

            HttpResponseMessage response = await _httpClient.GetAsync(prayerTimesURL);
            response.EnsureSuccessStatusCode();
            string jsonPrayerTimesString = await response.Content.ReadAsStringAsync();

            List<SemerkandPrayerTimes> allPrayerTimes = JsonConvert.DeserializeObject<List<SemerkandPrayerTimes>>(jsonPrayerTimesString, settings);

            foreach (SemerkandPrayerTimes prayerTime in allPrayerTimes)
            {
                DateTime currentPrayerTimeDate = 
                    new DateTime(date.Year, 1, 1)
                        .AddDays(prayerTime.DayOfYear-1);

                prayerTime.CityID = cityID;
                prayerTime.Date = currentPrayerTimeDate;
                prayerTime.Fajr = getCorrectDateTime(currentPrayerTimeDate, prayerTime.Fajr);
                prayerTime.Tulu = getCorrectDateTime(currentPrayerTimeDate, prayerTime.Tulu);
                prayerTime.Zuhr = getCorrectDateTime(currentPrayerTimeDate, prayerTime.Zuhr);
                prayerTime.Asr = getCorrectDateTime(currentPrayerTimeDate, prayerTime.Asr);
                prayerTime.Maghrib = getCorrectDateTime(currentPrayerTimeDate, prayerTime.Maghrib);
                prayerTime.Isha = getCorrectDateTime(currentPrayerTimeDate, prayerTime.Isha);
            }

            return allPrayerTimes.Where(pt => pt.Date == date.Date).ToList();
        }

        private DateTime getCorrectDateTime(DateTime actualDate, DateTime onlyDayTime)
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
