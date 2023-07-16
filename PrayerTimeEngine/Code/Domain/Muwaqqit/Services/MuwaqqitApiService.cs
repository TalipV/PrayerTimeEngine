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
        public MuwaqqitPrayerTimes GetTimes(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree, string timezone)
        {
            MuwaqqitPrayerTimes prayerTimes = null;

            using (HttpClient client = new HttpClient())
            {
                UriBuilder builder = new UriBuilder("https://www.muwaqqit.com/api2.json");
                NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);

                query["d"] = date.ToString("yyyy-MM-dd");
                query["ln"] = longitude.ToString(CultureInfo.InvariantCulture);
                query["lt"] = latitude.ToString(CultureInfo.InvariantCulture);
                query["tz"] = timezone;
                query["fa"] = fajrDegree.ToString(CultureInfo.InvariantCulture);
                query["ea"] = ishaDegree.ToString(CultureInfo.InvariantCulture);
                builder.Query = query.ToString();

                string url = builder.ToString();

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    // Parse the JSON response to the MuwaqqitResponse object
                    MuwaqqitResponse muwaqqitResponse = JsonSerializer.Deserialize<MuwaqqitResponse>(jsonResponse);

                    prayerTimes = new MuwaqqitPrayerTimes(
                        DateTimeOffset.Parse(muwaqqitResponse.d).DateTime,
                        (decimal)muwaqqitResponse.ln,
                        (decimal)muwaqqitResponse.lt,
                        DateTimeOffset.Parse(muwaqqitResponse.fajr).DateTime,
                        DateTimeOffset.Parse(muwaqqitResponse.sunrise).DateTime,
                        DateTimeOffset.Parse(muwaqqitResponse.zohr).DateTime,
                        DateTimeOffset.Parse(muwaqqitResponse.asr_shafi).DateTime,
                        DateTimeOffset.Parse(muwaqqitResponse.asr_hanafi).DateTime,
                        DateTimeOffset.Parse(muwaqqitResponse.sunset).DateTime,
                        DateTimeOffset.Parse(muwaqqitResponse.esha).DateTime);
                }
            }

            return prayerTimes;
        }
    }
}
