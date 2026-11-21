using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

public class DynamicPrayerTimeCalculationErrorVO
{
    public required EDynamicPrayerTimeProviderType DynamicPrayerTimeProviderType { get; init; }
    public required LocalDate Date { get; init; }
    public required List<ETimeType> TimeTypes { get; init; } = [];
    public required Exception Exception { get; init; }

    /// <summary>The provider simply has no data for that day; not a failure of the provider.</summary>
    public bool IsDateNotAvailable => Exception is PrayerTimesNotAvailableForDateException;
}
