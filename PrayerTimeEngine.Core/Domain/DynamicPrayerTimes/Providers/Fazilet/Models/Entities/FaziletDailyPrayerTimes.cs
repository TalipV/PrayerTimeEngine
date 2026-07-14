using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

public class FaziletDailyPrayerTimes : IDailyPrayerTimes, IEntity
{
    [Key]
    public int ID { get; set; }

    public required LocalDate Date { get; set; }
    public required DateTimeZone TimeZone { get; set; }
    public required int CityID { get; set; }
    public Instant? InsertInstant { get; set; }

    public required Instant? Imsak { get; set; }
    public required Instant? Fajr { get; set; }
    public required Instant? Shuruq { get; set; }
    public required Instant? Duha { get; set; }
    public required Instant? Dhuhr { get; set; }
    public required Instant? Asr { get; set; }
    public required Instant? Maghrib { get; set; }
    public required Instant? Isha { get; set; }
    public Instant? NextFajr { get; set; }

    public ZonedDateTime? GetZonedDateTimeForTimeType(ETimeType timeType)
    {
        Instant? instant = timeType switch
        {
            ETimeType.FajrStart => Fajr,
            ETimeType.FajrEnd => Shuruq,
            ETimeType.DuhaStart => Duha,
            ETimeType.DhuhrStart => Dhuhr,
            ETimeType.DhuhrEnd => Asr,
            ETimeType.AsrStart => Asr,
            ETimeType.AsrEnd => Maghrib,
            ETimeType.MaghribStart => Maghrib,
            ETimeType.MaghribEnd => Isha,
            ETimeType.IshaStart => Isha,
            ETimeType.IshaEnd => NextFajr,
            _ => throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}."),
        };

        return instant?.InZone(TimeZone);
    }
}
