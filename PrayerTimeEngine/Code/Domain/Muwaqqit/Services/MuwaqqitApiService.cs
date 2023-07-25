using PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Services
{
    public class MuwaqqitApiService : IMuwaqqitApiService
    {
        private readonly HttpClient _httpClient;

        public MuwaqqitApiService(HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }

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

            UriBuilder builder = new UriBuilder("https://www.muwaqqit.com/api2.json");
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

                prayerTimes = new MuwaqqitPrayerTimes(
                    DateTimeOffset.Parse(muwaqqitResponse.d).DateTime,
                    muwaqqitResponse.ln,
                    muwaqqitResponse.lt,
                    DateTimeOffset.Parse(muwaqqitResponse.fajr).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.fajr_t).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.sunrise).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.ishraq).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.zohr).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.asr_shafi).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.asr_hanafi).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.sunset).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.esha).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.ishtibak).DateTime,
                    DateTimeOffset.Parse(muwaqqitResponse.asr_makrooh).DateTime);
            }

            return prayerTimes;
        }
    }
}
