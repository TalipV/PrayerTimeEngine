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

    public required Instant? Fajr { get; set; }
    public required Instant? NextFajr { get; set; }
    public required Instant? Shuruq { get; set; }
    public required Instant? Duha { get; set; }
    public required Instant? Dhuhr { get; set; }
    public required Instant? Asr { get; set; }
    public required Instant? AsrMithlayn { get; set; }
    public required Instant? Maghrib { get; set; }
    public required Instant? Isha { get; set; }
    public required Instant? Ishtibaq { get; set; }
    public required Instant? AsrKaraha { get; set; }

    public ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType)
    {
        Instant? instant = timeType switch
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

        return instant?.InZone(TimeZone);
    }
}
