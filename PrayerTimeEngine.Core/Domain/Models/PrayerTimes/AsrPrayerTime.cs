﻿using NodaTime;

namespace PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

public class AsrPrayerTime : AbstractPrayerTime
{
    public override string Name => "Asr";
    public ZonedDateTime? Mithlayn { get; set; }
    public ZonedDateTime? Karaha { get; set; }
}