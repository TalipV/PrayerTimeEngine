using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;

/// <summary>
/// One cached day of a Waqti month response.
/// </summary>
/// <remarks>
/// <see cref="FajrAngle"/> and <see cref="IshaaAngle"/> are the angle overrides of the request
/// (null: the ones of the method). They are part of the cache key, because a day calculated with
/// overrides only serves the times derived from these angles (see <see cref="GetZonedDateTimeForTimeType"/>).
/// </remarks>
public class WaqtiDailyPrayerTimes : IDailyPrayerTimes, IEntity
{
    public int ID { get; set; }

    public required LocalDate Date { get; set; }
    public required DateTimeZone TimeZone { get; set; }
    public required decimal Longitude { get; set; }
    public required decimal Latitude { get; set; }
    public required string Method { get; set; }
    public double? FajrAngle { get; set; }
    public double? IshaaAngle { get; set; }
    public Instant? InsertInstant { get; set; }

    public required Instant? Fajr { get; set; }
    public required Instant? Shuruq { get; set; }
    public required Instant? Duha { get; set; }
    public required Instant? Dhuhr { get; set; }
    public required Instant? Asr { get; set; }
    public required Instant? AsrMithlayn { get; set; }
    public required Instant? Maghrib { get; set; }
    public required Instant? Isha { get; set; }
    public required Instant? NextFajr { get; set; }

    public ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType)
    {
        Instant? instant = timeType switch
        {
            // Ghalas and Karaha are Fajr calculated for their own sun angle
            ETimeType.FajrStart or ETimeType.FajrGhalas or ETimeType.FajrKaraha => Fajr,
            ETimeType.FajrEnd => Shuruq,
            ETimeType.DuhaStart => Duha,
            ETimeType.DhuhrStart => Dhuhr,
            ETimeType.DhuhrEnd => Asr,
            ETimeType.AsrStart => Asr,
            ETimeType.AsrEnd => Maghrib,
            ETimeType.AsrMithlayn => AsrMithlayn,
            ETimeType.MaghribStart => Maghrib,
            // Asr Karaha (sun above the horizon) and Ishtibak are Isha calculated for their own sun angle
            ETimeType.MaghribEnd or ETimeType.IshaStart or ETimeType.AsrKaraha or ETimeType.MaghribIshtibak => Isha,
            ETimeType.IshaEnd => NextFajr,
            _ => throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}."),
        };

        return instant?.InZone(TimeZone);
    }
}
