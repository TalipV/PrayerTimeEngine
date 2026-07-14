using NodaTime;
using NodaTime.TimeZones;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using System.Text.Json.Serialization;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;

public class SemerkandPrayerTimesResponseDTO
{
    [JsonPropertyName("DayOfYear")]
    public int DayOfYear { get; set; }

    [JsonPropertyName("Fajr")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Fajr { get; set; }

    [JsonPropertyName("Tulu")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Shuruq { get; set; }

    [JsonPropertyName("Zuhr")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Dhuhr { get; set; }

    [JsonPropertyName("Asr")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Asr { get; set; }

    [JsonPropertyName("Maghrib")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Maghrib { get; set; }

    [JsonPropertyName("Isha")]
    [JsonConverter(typeof(LocalTimeConverter))]
    public LocalTime? Isha { get; set; }

    internal SemerkandDailyPrayerTimes ToSemerkandPrayerTimes(int cityID, DateTimeZone dateTimeZone, LocalDate firstDayOfYear, SemerkandDailyPrayerTimes previousDay = null)
    {
        LocalDate localDate = firstDayOfYear.PlusDays(DayOfYear - 1);

        return new SemerkandDailyPrayerTimes
        {
            DayOfYear = DayOfYear,
            CityID = cityID,
            TimeZone = dateTimeZone,
            Date = localDate,
            Fajr = getInstant(localDate, dateTimeZone, Fajr, previousDay?.Fajr),
            Shuruq = getInstant(localDate, dateTimeZone, Shuruq, previousDay?.Shuruq),
            Dhuhr = getInstant(localDate, dateTimeZone, Dhuhr, previousDay?.Dhuhr),
            Asr = getInstant(localDate, dateTimeZone, Asr, previousDay?.Asr),
            Maghrib = getInstant(localDate, dateTimeZone, Maghrib, previousDay?.Maghrib),
            Isha = getInstant(localDate, dateTimeZone, Isha, previousDay?.Isha),
        };
    }

    private static Instant? getInstant(LocalDate date, DateTimeZone zone, LocalTime? time, Instant? prevDayHint)
    {
        if (time is null)
            return null;

        LocalDateTime localDateTime = date + time.Value;
        ZoneLocalMapping mapping = zone.MapLocal(localDateTime);

        // in 99.99% of cases there will be exactly one mapping but if a specific clock time
        // occurs twice at one day, like 02:30 on the 27.10.2026 in Europe/Berlin,
        // then we have to find out which one to use.
        // In the above example it won't be an issue because none of those prayer times falls within that 02:00-03:00 range
        // but these rare edge cases are still possible for specific places during specific times in specific timezones
        return mapping.Count switch
        {
            0 => throw new SkippedTimeException(localDateTime, zone),   // When the local time does not exist in the timezone, like when 02:00 jumps to 03:00 and then 02:30 is processed for that day
            1 => mapping.First().ToInstant(),
            2 => resolveAmbiguousTime(mapping, prevDayHint),
            _ => throw new InvalidOperationException("Unexpected mapping count.")
        };
    }

    private static Instant resolveAmbiguousTime(ZoneLocalMapping mapping, Instant? instantOfPreviousDay)
    {
        // nothing to compare it against
        if (instantOfPreviousDay is null)
            throw new AmbiguousTimeException(mapping.First(), mapping.Last());

        Instant earlierOption = mapping.First().ToInstant();
        Instant laterOption = mapping.Last().ToInstant();

        long distEarlier = Math.Abs((earlierOption - instantOfPreviousDay.Value).BclCompatibleTicks);
        long distLater = Math.Abs((laterOption - instantOfPreviousDay.Value).BclCompatibleTicks);

        // take whatever is closer to the previous day's time
        return distEarlier <= distLater 
            ? earlierOption 
            : laterOption;
    }
}
