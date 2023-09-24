using MethodTimer;
using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitApiService : IMuwaqqitApiService
    {
        private readonly HttpClient _httpClient;

        public MuwaqqitApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        internal const string MUWAQQIT_API_URL = @"https://www.muwaqqit.com/api2.json";

        [Time]
        public async Task<MuwaqqitPrayerTimes> GetTimesAsync(
            LocalDate date,
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

            query["d"] = date.ToString("yyyy-MM-dd", null);
            query["ln"] = longitude.ToString(CultureInfo.InvariantCulture);
            query["lt"] = latitude.ToString(CultureInfo.InvariantCulture);
            query["tz"] = timezone;

            query["fa"] = fajrDegree.ToString(CultureInfo.InvariantCulture);
            query["ia"] = asrKarahaDegree.ToString(CultureInfo.InvariantCulture);
            query["isn"] = ishtibaqDegree.ToString(CultureInfo.InvariantCulture);
            query["ea"] = ishaDegree.ToString(CultureInfo.InvariantCulture);

            builder.Query = query.ToString();

            string url = builder.ToString();

            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // Parse the JSON response to the MuwaqqitJSONResponse object
            MuwaqqitJSONResponse muwaqqitResponse = JsonSerializer.Deserialize<MuwaqqitJSONResponse>(jsonResponse);

            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[timezone];

            prayerTimes = new MuwaqqitPrayerTimes
            {
                Date = getZonedDateTime(muwaqqitResponse.d, dateTimeZone).Date,
                Longitude = muwaqqitResponse.ln,
                Latitude = muwaqqitResponse.lt,

                FajrDegree = fajrDegree,
                AsrKarahaDegree = asrKarahaDegree,
                IshtibaqDegree = ishtibaqDegree,
                IshaDegree = ishaDegree,

                Fajr = getZonedDateTime(muwaqqitResponse.fajr, dateTimeZone),
                NextFajr = getZonedDateTime(muwaqqitResponse.fajr_t, dateTimeZone),
                Shuruq = getZonedDateTime(muwaqqitResponse.sunrise, dateTimeZone),
                Duha = getZonedDateTime(muwaqqitResponse.ishraq, dateTimeZone),
                Dhuhr = getZonedDateTime(muwaqqitResponse.zohr, dateTimeZone),
                Asr = getZonedDateTime(muwaqqitResponse.asr_shafi, dateTimeZone),
                AsrMithlayn = getZonedDateTime(muwaqqitResponse.asr_hanafi, dateTimeZone),
                Maghrib = getZonedDateTime(muwaqqitResponse.sunset, dateTimeZone),
                Isha = getZonedDateTime(muwaqqitResponse.esha, dateTimeZone),
                Ishtibaq = getZonedDateTime(muwaqqitResponse.ishtibak, dateTimeZone),
                AsrKaraha = getZonedDateTime(muwaqqitResponse.asr_makrooh, dateTimeZone),
            };

            return prayerTimes;
        }

        private ZonedDateTime getZonedDateTime(string zonedDateTimeString, DateTimeZone dateTimeZone)
        {
            OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss.FFFFFFFo<G>")
                .Parse(zonedDateTimeString)
                .Value
                .Deconstruct(out LocalDateTime localDateTime, out Offset offset);

            // ignore fractions of seconds
            localDateTime = 
                localDateTime.Date + new LocalTime(localDateTime.Hour, localDateTime.Minute, localDateTime.Second);

            return new ZonedDateTime(localDateTime, dateTimeZone, offset);
        }
    }
}
