using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitApiService : IMuwaqqitApiService
    {
        private readonly HttpClient _httpClient;

        public MuwaqqitApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        internal const string MUWAQQIT_API_URL = @"https://www.muwaqqit.com/api2.json";

        public async Task<MuwaqqitPrayerTimes> GetTimesAsync(
            DateTime date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree,
            string timezone)
        {
            MuwaqqitPrayerTimes prayerTimes = null;

            UriBuilder builder = new UriBuilder(MUWAQQIT_API_URL);
            NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);

            query["d"] = date.ToString("yyyy-MM-dd");
            query["ln"] = longitude.ToString(CultureInfo.InvariantCulture);
            query["lt"] = latitude.ToString(CultureInfo.InvariantCulture);
            query["tz"] = timezone;

            query["fa"] = fajrDegree.ToString(CultureInfo.InvariantCulture);
            query["ea"] = ishaDegree.ToString(CultureInfo.InvariantCulture);
            query["isn"] = ishtibaqDegree.ToString(CultureInfo.InvariantCulture);
            query["ia"] = asrKarahaDegree.ToString(CultureInfo.InvariantCulture);

            builder.Query = query.ToString();

            string url = builder.ToString();

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Parse the JSON response to the MuwaqqitJSONResponse object
                MuwaqqitJSONResponse muwaqqitResponse = JsonSerializer.Deserialize<MuwaqqitJSONResponse>(jsonResponse);

                prayerTimes = new MuwaqqitPrayerTimes
                {
                    Date = DateTimeOffset.Parse(muwaqqitResponse.d).DateTime,
                    Longitude = muwaqqitResponse.ln,
                    Latitude = muwaqqitResponse.lt,
                    Fajr = DateTimeOffset.Parse(muwaqqitResponse.fajr).DateTime,
                    NextFajr = DateTimeOffset.Parse(muwaqqitResponse.fajr_t).DateTime,
                    Shuruq = DateTimeOffset.Parse(muwaqqitResponse.sunrise).DateTime,
                    Duha = DateTimeOffset.Parse(muwaqqitResponse.ishraq).DateTime,
                    Dhuhr = DateTimeOffset.Parse(muwaqqitResponse.zohr).DateTime,
                    Asr = DateTimeOffset.Parse(muwaqqitResponse.asr_shafi).DateTime,
                    AsrMithlayn = DateTimeOffset.Parse(muwaqqitResponse.asr_hanafi).DateTime,
                    Maghrib = DateTimeOffset.Parse(muwaqqitResponse.sunset).DateTime,
                    Isha = DateTimeOffset.Parse(muwaqqitResponse.esha).DateTime,
                    Ishtibaq = DateTimeOffset.Parse(muwaqqitResponse.ishtibak).DateTime,
                    AsrKaraha = DateTimeOffset.Parse(muwaqqitResponse.asr_makrooh).DateTime,
                };
            }

            return prayerTimes;
        }
    }
}
