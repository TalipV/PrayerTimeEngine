using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Presentation.Views;

public static class PrayerTimesDayExtensions
{
    /// <summary>
    /// The prayer time running at <paramref name="instant"/> or, in a gap between two prayer
    /// times, the next one. Shared by the graphic and the highlighted card, so both always show
    /// the same prayer.
    /// </summary>
    public static (ETimeSection Section, GenericPrayerTime Times) GetCurrentOrNextPrayerTime(this IPrayerTimesDay prayerTimesDay, Instant instant)
    {
        List<(ETimeSection Section, GenericPrayerTime Times)> allPrayerTimes = prayerTimesDay.AllPrayerTimes;

        (ETimeSection Section, GenericPrayerTime Times) currentPrayerTime = allPrayerTimes
                .FirstOrDefault(x => x.Times.Start?.ToInstant() <= instant && instant <= x.Times.End?.ToInstant());

        if (currentPrayerTime != default)
        {
            return currentPrayerTime;
        }

        return allPrayerTimes
                .OrderBy(x => x.Times.Start?.ToInstant())
                .FirstOrDefault(x => x.Times.Start?.ToInstant() > instant);
    }
}
