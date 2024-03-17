using NodaTime;
using NodaTime.Text;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitApiService(
            HttpClient httpClient
        ) : IMuwaqqitApiService
    {
        internal const string MUWAQQIT_API_URL = @"https://www.muwaqqit.com/api2.json";

        public async Task<MuwaqqitPrayerTimes> GetTimesAsync(
            LocalDate date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree,
            string timezone,
            CancellationToken cancellationToken)
        {
            MuwaqqitPrayerTimes prayerTimes;

            var builder = new UriBuilder(MUWAQQIT_API_URL);
            var query = HttpUtility.ParseQueryString(builder.Query);

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

            using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            MuwaqqitJSONResponse muwaqqitResponse = await JsonSerializer.DeserializeAsync<MuwaqqitJSONResponse>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[timezone];

            prayerTimes = new MuwaqqitPrayerTimes
            {
                Date = getLocalDate(muwaqqitResponse.d),
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

        private static LocalDate getLocalDate(string zonedDateTimeString)
        {
            OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss.FFFFFFFo<G>")
                .Parse(zonedDateTimeString)
                .Value
                .Deconstruct(out LocalDateTime localDateTime, out _);

            return localDateTime.Date;
        }

        private static ZonedDateTime getZonedDateTime(string zonedDateTimeString, DateTimeZone dateTimeZone)
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
