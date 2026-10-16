using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;

namespace PrayerTimeEngine.Core.Domain.PrayerTimeProgress;

/// <summary>A named point in time which divides a prayer time, e.g. Mithlayn within Asr.</summary>
public sealed record PrayerTimeSubTime(string Name, ZonedDateTime? Time);

/// <summary>
/// A prayer time reduced to what is needed to show its progress: when it starts, when it ends and
/// the sub times which divide it. Platform neutral, so the phone notification and the watch share it.
/// </summary>
public sealed record PrayerTimeWindow(
    ETimeSection Section,
    ZonedDateTime? Start,
    ZonedDateTime? End,
    IReadOnlyList<PrayerTimeSubTime> SubTimes)
{
    public static PrayerTimeWindow FromPrayerTime(ETimeSection section, GenericPrayerTime prayerTime)
    {
        return new PrayerTimeWindow(section, prayerTime?.Start, prayerTime?.End, GetSubTimes(prayerTime));
    }

    public static List<PrayerTimeWindow> FromPrayerTimes(IEnumerable<(ETimeSection Section, GenericPrayerTime Times)> prayerTimes)
    {
        return [.. prayerTimes.Select(x => FromPrayerTime(x.Section, x.Times))];
    }

    /// <summary>The sub times which divide a prayer time, in no particular order.</summary>
    private static List<PrayerTimeSubTime> GetSubTimes(GenericPrayerTime prayerTime)
    {
        return prayerTime switch
        {
            FajrPrayerTime fajr => [new("Ghalas", fajr.Ghalas), new("Redness", fajr.Karaha)],
            DuhaPrayerTime duha => [new("1/4", duha.QuarterOfDay), new("1/2", duha.HalfOfDay)],
            AsrPrayerTime asr => [new("Mithlayn", asr.Mithlayn), new("Karaha", asr.Karaha)],
            MaghribPrayerTime maghrib => [new("Sufficient", maghrib.SufficientTime), new("Ishtibak", maghrib.Ishtibak)],
            IshaPrayerTime isha =>
            [
                new("1/3", isha.FirstThirdOfNight),
                new("1/2", isha.MiddleOfNight),
                new("2/3", isha.SecondThirdOfNight)
            ],
            _ => []
        };
    }
}
