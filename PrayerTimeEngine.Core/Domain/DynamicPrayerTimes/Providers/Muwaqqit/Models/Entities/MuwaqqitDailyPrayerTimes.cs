using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;

public class MuwaqqitDailyPrayerTimes : IDailyPrayerTimes, IEntity
{
    [Key]
    public int ID { get; set; }

    public required LocalDate Date { get; set; }
    public required DateTimeZone TimeZone { get; set; }
    public required decimal Longitude { get; set; }
    public required decimal Latitude { get; set; }
    public Instant? InsertInstant { get; set; }

    public required double FajrDegree { get; set; }
    public required double AsrKarahaDegree { get; set; }
    public required double IshtibaqDegree { get; set; }
    public required double IshaDegree { get; set; }

    public required LocalDateTime? Fajr { get; set; }
    public required LocalDateTime? NextFajr { get; set; }
    public required LocalDateTime? Shuruq { get; set; }
    public required LocalDateTime? Duha { get; set; }
    public required LocalDateTime? Dhuhr { get; set; }
    public required LocalDateTime? Asr { get; set; }
    public required LocalDateTime? AsrMithlayn { get; set; }
    public required LocalDateTime? Maghrib { get; set; }
    public required LocalDateTime? Isha { get; set; }
    public required LocalDateTime? Ishtibaq { get; set; }
    public required LocalDateTime? AsrKaraha { get; set; }

    public ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType)
    {
        LocalDateTime? localDateTime = timeType switch
        {
            ETimeType.FajrStart or ETimeType.FajrGhalas or ETimeType.FajrKaraha => Fajr,
            ETimeType.FajrEnd => Shuruq,
            ETimeType.DuhaStart => Duha,
            ETimeType.DhuhrStart or ETimeType.DuhaEnd => Dhuhr,
            ETimeType.DhuhrEnd => Asr,
            ETimeType.AsrStart => Asr,
            ETimeType.AsrEnd => Maghrib,
            ETimeType.AsrMithlayn => AsrMithlayn,
            ETimeType.AsrKaraha => AsrKaraha,
            ETimeType.MaghribStart => Maghrib,
            ETimeType.MaghribIshtibaq => Ishtibaq,
            ETimeType.MaghribEnd or ETimeType.IshaStart => Isha,
            ETimeType.IshaEnd => NextFajr,
            _ => throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}."),
        };

        // the offset is derived from the zone (lenient so ambiguous/skipped DST hours never throw)
        return localDateTime?.InZoneStrictly(TimeZone);
    }
}
