﻿using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

public class FaziletDailyPrayerTimes : IDailyPrayerTimes, IInsertedAt
{
    [Key]
    public int ID { get; set; }

    public required ZonedDateTime Date { get; set; }
    public required int CityID { get; set; }
    public Instant? InsertInstant { get; set; }

    public required ZonedDateTime Imsak { get; set; }
    public required ZonedDateTime Fajr { get; set; }
    public required ZonedDateTime Shuruq { get; set; }
    public required ZonedDateTime Duha { get; set; }
    public required ZonedDateTime Dhuhr { get; set; }
    public required ZonedDateTime Asr { get; set; }
    public required ZonedDateTime Maghrib { get; set; }
    public required ZonedDateTime Isha { get; set; }
    public ZonedDateTime? NextFajr { get; set; }

    public ZonedDateTime GetZonedDateTimeForTimeType(ETimeType timeType)
    {
        return timeType switch
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
            ETimeType.IshaEnd => NextFajr ?? Isha,
            _ => throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}."),
        };
    }
}
