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

    internal SemerkandDailyPrayerTimes ToSemerkandPrayerTimes(int cityID, DateTimeZone dateTimeZone, LocalDate firstDayOfYear, SemerkandDailyPrayerTimes previousDayPrayerTimes = null)
    {
        LocalDate localDate = firstDayOfYear.PlusDays(DayOfYear - 1);

        return new SemerkandDailyPrayerTimes
        {
            DayOfYear = DayOfYear,
            CityID = cityID,
            TimeZone = dateTimeZone,
            Date = localDate,
            Fajr = getInstant(localDate, dateTimeZone, Fajr, previousDayPrayerTimes?.Date, previousDayPrayerTimes?.Fajr),
            Shuruq = getInstant(localDate, dateTimeZone, Shuruq, previousDayPrayerTimes?.Date, previousDayPrayerTimes?.Shuruq),
            Dhuhr = getInstant(localDate, dateTimeZone, Dhuhr, previousDayPrayerTimes?.Date, previousDayPrayerTimes?.Dhuhr),
            Asr = getInstant(localDate, dateTimeZone, Asr, previousDayPrayerTimes?.Date, previousDayPrayerTimes?.Asr),
            Maghrib = getInstant(localDate, dateTimeZone, Maghrib, previousDayPrayerTimes?.Date, previousDayPrayerTimes?.Maghrib),
            Isha = getInstant(localDate, dateTimeZone, Isha, previousDayPrayerTimes?.Date, previousDayPrayerTimes?.Isha),
        };
    }

    private static Instant? getInstant(LocalDate date, DateTimeZone zone, LocalTime? time, LocalDate? previousDayDate, Instant? previousDayInstant)
    {
        if (time is null)
            return null;

        LocalDateTime localDateTime = date + time.Value;
        ZoneLocalMapping mapping = zone.MapLocal(localDateTime);

        // in 99.99% of cases there will be exactly one mapping but if a specific clock time
        // occurs twice at one day, like 02:30 on the 27.10.2024 in Europe/Berlin,
        // then we have to find out which one to use.
        // In the above example it won't be an issue because none of those prayer times falls within that 02:00-03:00 range
        // but these rare edge cases are still possible for specific places during specific times in specific timezones
        return mapping.Count switch
        {
            0 => throw new SkippedTimeException(localDateTime, zone),   // When the local time does not exist in the timezone, like when 02:00 jumps to 03:00 and then 02:30 is processed for that day
            1 => mapping.First().ToInstant(),
            2 => resolveAmbiguousTime(mapping, date, previousDayDate, previousDayInstant),
            _ => throw new InvalidOperationException("Unexpected mapping count.")
        };
    }

    private static readonly Duration _oneDayDuration = Duration.FromDays(1);

    private static Instant resolveAmbiguousTime(ZoneLocalMapping mapping, LocalDate date, LocalDate? previousDayDate, Instant? previousDayInstant)
    {
        // the comparison below only works with the direct predecessor
        if (previousDayDate is not null && previousDayDate != date.PlusDays(-1))
            throw new ArgumentException($"The previous day must be exactly one day before {date} but was {previousDayDate}.");

        // nothing to compare it against
        if (previousDayInstant is null)
            throw new AmbiguousTimeException(mapping.First(), mapping.Last());

        // Prayer times drift only by a few minutes per day, so the previous day's time plus 24h
        // is a good approximation of today's time and thereby suitable to pick between the
        // two options (which are exactly one hour apart).
        // The previous day's time itself is NOT suitable because it lies ~24h before both
        // options and would thus always be closer to the earlier one.
        Instant comparisonInstant = previousDayInstant.Value + _oneDayDuration;

        Instant earlierOption = mapping.First().ToInstant();
        Instant laterOption = mapping.Last().ToInstant();

        long distEarlier = Math.Abs((earlierOption - comparisonInstant).BclCompatibleTicks);
        long distLater = Math.Abs((laterOption - comparisonInstant).BclCompatibleTicks);

        // take whatever option is closer to the comparison value
        return distEarlier <= distLater
            ? earlierOption
            : laterOption;
    }
}
